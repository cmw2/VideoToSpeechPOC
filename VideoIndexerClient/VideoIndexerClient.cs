using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using VideoIndexer.Options;
using VideoIndexer.Auth;
using VideoIndexer.Model;
using VideoIndexer.Utils;

namespace VideoIndexer
{
    public class VideoIndexerClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<VideoIndexerClient> _logger;
        private readonly AccountTokenProvider _accountTokenProvider;
        private readonly AzureVideoIndexerOptions _options;

        private Lazy<Task<Account>> _accountLazy;

        private string _armAccessToken;
        private string _accountAccessToken;
        private Account _account;
        
        private readonly TimeSpan _pollingInteval = TimeSpan.FromSeconds(10);

        public VideoIndexerClient(IOptions<AzureVideoIndexerOptions> options, HttpClient httpClient, ILogger<VideoIndexerClient> logger, AccountTokenProvider accountTokenProvider)
        {
            _options = options.Value;
            _accountTokenProvider = accountTokenProvider;
            System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls13;
            _httpClient = httpClient;
            _logger = logger;
            _accountLazy = new Lazy<Task<Account>>(InitializeAccountAsync);
        }

        public async Task AuthenticateAsync()
        {
            try
            {
                _armAccessToken = await _accountTokenProvider.GetArmAccessTokenAsync();
                _accountAccessToken = await _accountTokenProvider.GetAccountAccessTokenAsync(_armAccessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed.");
                throw;
            }
        }

        private async Task<Account> InitializeAccountAsync()
        {
            await AuthenticateAsync();
            return await GetAccountAsync(_options.AccountName);
        }

        private async Task EnsureAccountInitializedAsync()
        {
            if (!_accountLazy.IsValueCreated)
            {
                _account = await _accountLazy.Value;
            }
        }

        /// <summary>
        /// Get Information about the Account
        /// </summary>
        /// <param name="accountName"></param>
        /// <returns></returns>
        public async Task<Account> GetAccountAsync(string accountName)
        {
            if (_account != null)
            {
                return _account;
            }
            _logger.LogInformation($"Getting account {accountName}.");
            try
            {
                // Set request uri
                var requestUri = $"{_options.ARMBaseUrl}/subscriptions/{_options.SubscriptionId}/resourcegroups/{_options.ResourceGroupName}/providers/Microsoft.VideoIndexer/accounts/{accountName}?api-version={_options.ARMApiVersion}";
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _armAccessToken);

                var result = await _httpClient.SendAsync(request);
                result.VerifyStatus(System.Net.HttpStatusCode.OK);

                var jsonResponseBody = await result.Content.ReadAsStringAsync();
                var account = JsonSerializer.Deserialize<Account>(jsonResponseBody);
                VerifyValidAccount(account, accountName);
                _logger.LogInformation($"[Account Details] Id:{account.Properties.Id}, Location: {account.Location}");
                _account = account;
                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAccount error");
                throw;
            }
        }

        /// <summary>
        /// Uploads a video and starts the video index. Calls the uploadVideo API (https://api-portal.videoindexer.ai/api-details#api=Operations&operation=Upload-Video)
        /// </summary>
        /// <param name="videoUrl"> Link To Publicy Accessed Video URL</param>
        /// <param name="videoName"> The Asset name to be used </param>
        /// <param name="exludedAIs"> The ExcludeAI list to run </param>
        /// <param name="waitForIndex"> should this method wait for index operation to complete </param>
        /// <exception cref="Exception"></exception>
        /// <returns> Video Id of the video being indexed, otherwise throws excpetion</returns>
        public async Task<string> UploadUrlAsync(string videoUrl , string videoName, string exludedAIs = null, bool waitForIndex = false )
        {
            await EnsureAccountInitializedAsync();

            _logger.LogInformation($"Video for account {_account.Properties.Id} is starting to upload.");
            
            try
            {
                //Build Query Parameter Dictionary
                var queryDictionary = new Dictionary<string, string>
                {
                    { "name", videoName },
                    { "description", "video_description" },
                    { "privacy", "private" },
                    { "accessToken" , _accountAccessToken },
                    { "videoUrl" , videoUrl }
                };

                if (!Uri.IsWellFormedUriString(videoUrl, UriKind.Absolute))
                {
                    throw new ArgumentException("VideoUrl or LocalVidePath are invalid");
                }
                
                var queryParams = queryDictionary.CreateQueryString();
                if (!string.IsNullOrEmpty(exludedAIs))
                    queryParams += AddExcludedAIs(exludedAIs);

                // Send POST request
                var url = $"{_options.ApiEndpoint}/{_account.Location}/Accounts/{_account.Properties.Id}/Videos?{queryParams}";
                var uploadRequestResult = await _httpClient.PostAsync(url, null);
                uploadRequestResult.VerifyStatus(System.Net.HttpStatusCode.OK);
                var uploadResult = await uploadRequestResult.Content.ReadAsStringAsync();

                // Get the video ID from the upload result
                var videoId = JsonSerializer.Deserialize<Video>(uploadResult).Id;
                _logger.LogInformation($"Video ID {videoId} was uploaded successfully");
                
                if (waitForIndex)
                {
                    _logger.LogInformation("Waiting for Index Operation to Complete");
                    await WaitForIndexAsync(videoId);
                }
                return videoId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Uploading Url");
                throw;
            }
        }

        /// <summary>
        /// Calls getVideoIndex API in 10 second intervals until the indexing state is 'processed'(https://api-portal.videoindexer.ai/api-details#api=Operations&operation=Get-Video-Index)
        /// </summary>
        /// <param name="videoId"> The video id </param>
        /// <exception cref="Exception"></exception>
        /// <returns> Prints video index when the index is complete, otherwise throws exception </returns>
        public async Task WaitForIndexAsync(string videoId)
        {
            await EnsureAccountInitializedAsync();

            _logger.LogInformation($"Waiting for video {videoId} to finish indexing.");
            while (true)
            {
                var queryParams = new Dictionary<string, string>()
                {
                    {"language", "English"},
                    { "accessToken" , _accountAccessToken },
                }.CreateQueryString();

                var requestUrl = $"{_options.ApiEndpoint}/{_account.Location}/Accounts/{_account.Properties.Id}/Videos/{videoId}/Index?{queryParams}";
                var videoGetIndexRequestResult = await _httpClient.GetAsync(requestUrl);
                videoGetIndexRequestResult.VerifyStatus(System.Net.HttpStatusCode.OK);
                var videoGetIndexResult = await videoGetIndexRequestResult.Content.ReadAsStringAsync();
                var processingState = JsonSerializer.Deserialize<Video>(videoGetIndexResult).State;

                // If job is finished
                if (processingState == ProcessingState.Processed.ToString())
                {
                    _logger.LogInformation($"The video index has completed. Here is the full JSON of the index for video ID {videoId}: \n{videoGetIndexResult}");
                    return;
                }
                else if (processingState == ProcessingState.Failed.ToString())
                {
                    _logger.LogInformation($"The video index failed for video ID {videoId}.");
                    throw new Exception(videoGetIndexResult);
                }

                // Job hasn't finished
                _logger.LogInformation($"The video index state is {processingState}");
                await Task.Delay(_pollingInteval);
            }
        }

        /// <summary>
        /// Searches for the video in the account. Calls the searchVideo API (https://api-portal.videoindexer.ai/api-details#api=Operations&operation=Search-Videos)
        /// </summary>
        /// <param name="videoId"> The video id </param>
        /// <returns> Prints the video metadata, otherwise throws excpetion</returns>
        public async Task GetVideoAsync(string videoId)
        {
            await EnsureAccountInitializedAsync();

            _logger.LogInformation($"Searching videos in account {_account.Properties.Id} for video ID {videoId}.");
            var queryParams = new Dictionary<string, string>()
            {
                {"id", videoId},
                { "accessToken" , _accountAccessToken },
            }.CreateQueryString();

            try
            {
                var requestUrl = $"{_options.ApiEndpoint}/{_account.Location}/Accounts/{_account.Properties.Id}/Videos/Search?{queryParams}";
                var searchRequestResult = await _httpClient.GetAsync(requestUrl);
                searchRequestResult.VerifyStatus(System.Net.HttpStatusCode.OK);
                var searchResult = await searchRequestResult.Content.ReadAsStringAsync();
                _logger.LogInformation($"Here are the search results: \n{searchResult}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Video");
            }
        }

        public async Task<string> FileUploadAsync(string videoName,  string mediaPath, string exludedAIs = null)
        {
            await EnsureAccountInitializedAsync();

            if (!File.Exists(mediaPath))
                throw new Exception($"Could not find file at path {mediaPath}");

            var queryParams = new Dictionary<string, string>
            {
                { "name", videoName },
                { "description", "video_description" },
                { "privacy", "private" },
                { "accessToken" , _accountAccessToken },
                { "partition", "partition" }
            }.CreateQueryString();
            
            if (!string.IsNullOrEmpty(exludedAIs))
                queryParams += AddExcludedAIs(exludedAIs);
            
            var url = $"{_options.ApiEndpoint}/{_account.Location}/Accounts/{_account.Properties.Id}/Videos?{queryParams}";
            // Create multipart form data content
            using var content = new MultipartFormDataContent();
            // Add file content
            await using var fileStream = new FileStream(mediaPath, FileMode.Open, FileAccess.Read);
            using var streamContent = new StreamContent(fileStream);
            content.Add(streamContent, "fileName", Path.GetFileName(mediaPath));
            _logger.LogInformation("Uploading a local file using multipart/form-data post request..");
            // Send POST request
            var response = await _httpClient.PostAsync(url, content);
            _logger.LogInformation(response.Headers.ToString());
            // Process response
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            _logger.LogInformation($"Request failed with status code: {response.StatusCode}");
            return response.ToString();
        }

        /// <summary>
        /// Calls the getVideoInsightsWidget API (https://api-portal.videoindexer.ai/api-details#api=Operations&operation=Get-Video-Insights-Widget)
        /// </summary>
        /// <param name="videoId"> The video id </param>
        /// <returns> Prints the VideoInsightsWidget URL, otherwise throws exception</returns>
        public async Task GetInsightsWidgetUrlAsync(string videoId)
        {
            await EnsureAccountInitializedAsync();

            var videoAccessToken = await _accountTokenProvider.GetVideoAccessTokenAsync(_armAccessToken, videoId);
            _logger.LogInformation($"Getting the insights widget URL for video {videoId}");
            var queryParams = new Dictionary<string, string>()
            {
                {"widgetType", "Keywords"},
                { "accessToken" , videoAccessToken },
                {"allowEdit", "true"},
            }.CreateQueryString();
            try
            {
                var requestUrl = $"{_options.ApiEndpoint}/{_account.Location}/Accounts/{_account.Properties.Id}/Videos/{videoId}/InsightsWidget?{queryParams}";
                var insightsWidgetRequestResult = await _httpClient.GetAsync(requestUrl);
                insightsWidgetRequestResult.VerifyStatus(System.Net.HttpStatusCode.MovedPermanently);
                var insightsWidgetLink = insightsWidgetRequestResult.Headers.Location;
                _logger.LogInformation($"Got the insights widget URL: \n{insightsWidgetLink}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Getting Insights Widget");
            }
        }

        /// <summary>
        /// Calls the getVideoPlayerWidget API (https://api-portal.videoindexer.ai/api-details#api=Operations&operation=Get-Video-Player-Widget)
        /// </summary>
        /// <param name="videoId"> The video id </param>
        /// <returns> Prints the VideoPlayerWidget URL, otherwise throws exception</returns>
        public async Task GetPlayerWidgetUrlAsync( string videoId)
        {
            await EnsureAccountInitializedAsync();

            _logger.LogInformation($"Getting the player widget URL for video {videoId}");
            var videoAccessToken = await _accountTokenProvider.GetVideoAccessTokenAsync(_armAccessToken, videoId);
            var queryParams = new Dictionary<string, string>()
            {
                { "accessToken" , videoAccessToken }
            }.CreateQueryString();
            try
            {
                var requestUrl = $"{_options.ApiEndpoint}/{_account.Location}/Accounts/{_account.Properties.Id}/Videos/{videoId}/PlayerWidget?{queryParams}";
                var playerWidgetRequestResult = await _httpClient.GetAsync(requestUrl);

                var playerWidgetLink = playerWidgetRequestResult.Headers.Location;
                playerWidgetRequestResult.VerifyStatus(System.Net.HttpStatusCode.MovedPermanently);
                _logger.LogInformation($"Got the player widget URL: \n{playerWidgetLink}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Player Widget");
            }
        }

        public async Task<List<Video>> GetVideosAsync()
        {
            await EnsureAccountInitializedAsync();

            _logger.LogInformation($"Retrieving videos for account {_account.Properties.Id}.");

            try
            {
                var queryParams = new Dictionary<string, string>
                {
                    { "accessToken", _accountAccessToken }
                }.CreateQueryString();

                var requestUrl = $"{_options.ApiEndpoint}/{_account.Location}/Accounts/{_account.Properties.Id}/Videos?{queryParams}";
                var response = await _httpClient.GetAsync(requestUrl);
                response.VerifyStatus(System.Net.HttpStatusCode.OK);
                var responseBody = await response.Content.ReadAsStringAsync();
                var videoResponse = JsonSerializer.Deserialize<VideoResponse>(responseBody);
                return videoResponse?.Results ?? new List<Video>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving videos");
                throw;
            }
        }

        private string AddExcludedAIs(string ExcludedAI)
        {
            if (string.IsNullOrEmpty(ExcludedAI))
            {
                return "";
            }
            var list = ExcludedAI.Split(',');
            return list.Aggregate("", (current, item) => current + ("&ExcludedAI=" + item));
        }

        private void VerifyValidAccount(Account account,string accountName)
        {
            if (string.IsNullOrWhiteSpace(account.Location) || account.Properties == null || string.IsNullOrWhiteSpace(account.Properties.Id))
            {
                _logger.LogError($"{nameof(accountName)} {accountName} not found. Check {nameof(_options.SubscriptionId)}, {nameof(_options.ResourceGroupName)}, {nameof(accountName)} are valid.");
                throw new Exception($"Account {accountName} not found.");
            }
        }

    }
}
