using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using VideoIndexer;
using VideoIndexer.Model;
using VideoToSpeechPOC.Data;

namespace VideoToSpeechPOC.Components.Pages
{
    public partial class Home
    {
        [Inject]
        private VideoIndexerClient VideoIndexerClient { get; set; } = default!;

        [Inject]
        private ILogger<Home> Logger { get; set; } = default!;

        [Inject]
        private AzureSpeechService SpeechService { get; set; }

        private List<Video> videos = new();
        private string statusMessage = string.Empty;
        private bool isProcessing = false;
        [SupplyParameterFromForm]
        private string? selectedVideoId { get; set; }
        private Dictionary<string, VideoSummaryInfoViewModel>? selectedVideoSummaries;
        private string? SelectedVideoId
        {
            get => selectedVideoId;
            set
            {
                if (selectedVideoId != value)
                {
                    selectedVideoId = value;
                    _ = OnVideoSelected(selectedVideoId);
                }
            }
        }

        private string summaryLength = "Medium"; // Default to Medium
        private string summaryStyle = "Neutral"; // Default to Neutral

        protected override async Task OnInitializedAsync()
        {
            if (videos.Count == 0)
            {
                statusMessage = "Loading videos...";
                StateHasChanged(); // Trigger a re-render to show the loading message

                try
                {
                    videos = await VideoIndexerClient.GetVideosAsync();
                    statusMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    statusMessage = $"Error loading videos: {ex.Message}";
                    Logger.LogError(ex, "Error loading videos");
                }

                StateHasChanged(); // Trigger a re-render to update the UI with the videos or error message
            }
        }

        private async Task OnVideoSelected(string? videoId)
        {
            if (!string.IsNullOrEmpty(videoId))
            {
                var summaries = await VideoIndexerClient.GetVideoSummaryAsync(videoId);
                selectedVideoSummaries = summaries.ToDictionary(s => s.Id, s => new VideoSummaryInfoViewModel { Summary = s.Summary });
                StateHasChanged(); // Trigger a re-render to show the selected video details and summary
            }
            else
            {
                selectedVideoSummaries = null;
            }
        }

        private async Task RequestSummaryAsync()
        {
            if (!string.IsNullOrEmpty(SelectedVideoId))
            {
                await VideoIndexerClient.RequestVideoSummaryAsync(SelectedVideoId, length: summaryLength, style: summaryStyle);
                statusMessage = "Summary request submitted. Please wait for the summary to be generated.";
                StateHasChanged(); // Trigger a re-render to show the status message
            }
        }

        private string FormatDuration(double durationInSeconds)
        {
            var timeSpan = TimeSpan.FromSeconds(durationInSeconds);
            return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
        }

        private async Task GenerateAudio(string summaryId)
        {
            if (selectedVideoSummaries != null && selectedVideoSummaries.TryGetValue(summaryId, out var summaryInfo))
            {
                var llmOutput = summaryInfo.Summary;
                if (!string.IsNullOrEmpty(llmOutput))
                {
                    isProcessing = true;
                    statusMessage = "Processing...";
                    try
                    {
                        summaryInfo.AudioFileToken = await SpeechService.SynthesizeSpeechToFileAsync(llmOutput);
                        statusMessage = "Audio file generated successfully.";
                    }
                    catch (Exception ex)
                    {
                        statusMessage = $"Error: {ex.Message}";
                        Logger.LogError(ex, "Error generating audio");
                    }
                    finally
                    {
                        isProcessing = false;
                        StateHasChanged(); // Trigger a re-render to update the UI
                    }
                }
            }
        }
    }
}
