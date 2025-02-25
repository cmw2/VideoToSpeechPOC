using System;
using System.Net.Http;
using Azure.Identity;
using Azure.Core;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VideoIndexer.Utils;
using Microsoft.Extensions.Options;
using VideoIndexer.Options;
using Microsoft.Extensions.Logging;

namespace VideoIndexer.Auth
{

    public class AccountTokenProvider
    {
        private AzureVideoIndexerOptions _options;
        private readonly ILogger<AccountTokenProvider> _logger;
        private readonly HttpClient _httpClient;

        public AccountTokenProvider(IOptions<AzureVideoIndexerOptions> _options, ILogger<AccountTokenProvider> logger, HttpClient httpClient)
        {
            this._options = _options.Value;
            this._logger = logger;
            this._httpClient = httpClient;
        }

        public async Task<string> GetArmAccessTokenAsync(CancellationToken ct = default)
        {

            var credentials = GetTokenCredential();
            var tokenRequestContext = new TokenRequestContext(new[] { $"{_options.ARMBaseUrl}/.default" });
            var tokenRequestResult = await credentials.GetTokenAsync(tokenRequestContext, ct);
            return tokenRequestResult.Token;
        }


        public async Task<string> GetAccountAccessTokenAsync(string armAccessToken, ArmAccessTokenPermission permission = ArmAccessTokenPermission.Contributor, ArmAccessTokenScope scope = ArmAccessTokenScope.Account, CancellationToken ct = default)
        {
            var accessTokenRequest = new AccessTokenRequest
            {
                PermissionType = permission,
                Scope = scope
            };

            try
            {
                var jsonRequestBody = JsonSerializer.Serialize(accessTokenRequest);
                _logger.LogInformation($"Getting Account access token: {jsonRequestBody}");
                var httpContent = new StringContent(jsonRequestBody, System.Text.Encoding.UTF8, "application/json");

                // Set request uri
                var requestUri = $"{_options.ARMBaseUrl}/subscriptions/{_options.SubscriptionId}/resourcegroups/{_options.ResourceGroupName}/providers/Microsoft.VideoIndexer/accounts/{_options.AccountName}/generateAccessToken?api-version={_options.ARMApiVersion}";
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", armAccessToken);

                var result = await _httpClient.PostAsync(requestUri, httpContent, ct);
                result.EnsureSuccessStatusCode();
                var jsonResponseBody = await result.Content.ReadAsStringAsync(ct);
                _logger.LogInformation($"Got Account access token: {scope} , {permission}");
                return JsonSerializer.Deserialize<GenerateAccessTokenResponse>(jsonResponseBody)?.AccessToken!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Account access token.");
                throw;
            }
        }

        public async Task<string> GetVideoAccessTokenAsync(string armAccessToken, string videoId, ArmAccessTokenPermission permission = ArmAccessTokenPermission.Contributor, ArmAccessTokenScope scope = ArmAccessTokenScope.Video, CancellationToken ct = default)
        {
            var accessTokenRequest = new AccessTokenRequest
            {
                PermissionType = permission,
                Scope = scope,
                VideoId = videoId
            };

            try
            {
                var jsonRequestBody = JsonSerializer.Serialize(accessTokenRequest);
                _logger.LogInformation($"Getting Video access token: {jsonRequestBody}");
                var httpContent = new StringContent(jsonRequestBody, System.Text.Encoding.UTF8, "application/json");

                // Set request uri
                var requestUri = $"{_options.ARMBaseUrl}/subscriptions/{_options.SubscriptionId}/resourcegroups/{_options.ResourceGroupName}/providers/Microsoft.VideoIndexer/accounts/{_options.AccountName}/generateAccessToken?api-version={_options.ARMApiVersion}";
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", armAccessToken);

                var result = await _httpClient.PostAsync(requestUri, httpContent, ct);
                result.EnsureSuccessStatusCode();
                var jsonResponseBody = await result.Content.ReadAsStringAsync(ct);
                _logger.LogInformation($"Got Video access token: {scope} , {permission}");
                return JsonSerializer.Deserialize<GenerateAccessTokenResponse>(jsonResponseBody)?.AccessToken!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Video access token.");
                throw;
            }
        }

        private TokenCredential GetTokenCredential()
        {
            if (!string.IsNullOrEmpty(_options.ClientId) && !string.IsNullOrEmpty(_options.ClientSecret))
            {
                return new ClientSecretCredential(_options.TenantId, _options.ClientId, _options.ClientSecret);
            }
            else
            {
                var credentialOptions = _options.TenantId == null ? new DefaultAzureCredentialOptions() : new DefaultAzureCredentialOptions
                {
                    VisualStudioTenantId = _options.TenantId,
                    VisualStudioCodeTenantId = _options.TenantId,
                    SharedTokenCacheTenantId = _options.TenantId,
                    InteractiveBrowserTenantId = _options.TenantId
                };

                return new DefaultAzureCredential(credentialOptions);
            }
        }


    }
}
