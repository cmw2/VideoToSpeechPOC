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
        [SupplyParameterFromForm]
        private string newVideoUrl { get; set; } = string.Empty;
        [SupplyParameterFromForm]
        private string newVideoName { get; set; } = string.Empty;
        [SupplyParameterFromForm]
        private string newVideoDescription { get; set; } = string.Empty;
        private EditContext editContext;
        private ValidationMessageStore validationMessageStore;

        protected override async Task OnInitializedAsync()
        {
            editContext = new EditContext(this);
            validationMessageStore = new ValidationMessageStore(editContext);

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

        private async Task HandleValidSubmit()
        {
            try
            {
                statusMessage = "Uploading video...";
                StateHasChanged();

                var videoId = await VideoIndexerClient.UploadUrlAsync(newVideoUrl, newVideoName, newVideoDescription);
                statusMessage = $"Video uploaded successfully with ID: {videoId}";

                // Refresh the video list
                videos = await VideoIndexerClient.GetVideosAsync();

                // Clear the form fields
                newVideoUrl = string.Empty;
                newVideoName = string.Empty;
                newVideoDescription = string.Empty;

                // Notify the EditContext that the validation state has changed
                editContext.NotifyValidationStateChanged();
            }
            catch (Exception ex)
            {
                statusMessage = $"Error uploading video: {ex.Message}";
                Logger.LogError(ex, "Error uploading video");

                // Add validation message
                validationMessageStore.Clear();
                validationMessageStore.Add(() => newVideoUrl, ex.Message);
                editContext.NotifyValidationStateChanged();
            }

            StateHasChanged();
        }
    }
}
