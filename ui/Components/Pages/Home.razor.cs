using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using VideoIndexer;
using VideoIndexer.Model;

namespace VideoToSpeechPOC.Components.Pages
{
    public partial class Home
    {
        [Inject]
        private VideoIndexerClient VideoIndexerClient { get; set; } = default!;

        [Inject]
        private ILogger<Home> Logger { get; set; } = default!;

        private List<Video> videos = new();
        private string statusMessage = string.Empty;

        protected override async Task OnInitializedAsync()
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

        private string FormatDuration(double durationInSeconds)
        {
            var timeSpan = TimeSpan.FromSeconds(durationInSeconds);
            return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
        }
    }
}
