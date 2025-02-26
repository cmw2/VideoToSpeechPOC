using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using VideoIndexer;
using VideoIndexer.Model;

namespace VideoToSpeechPOC.Components.Pages
{
    public partial class AddVideoByUrl
    {
        [Inject]
        private VideoIndexerClient VideoIndexerClient { get; set; } = default!;

        [Inject]
        private ILogger<AddVideoByUrl> Logger { get; set; } = default!;

        private string statusMessage = string.Empty;
        [SupplyParameterFromForm]
        private string newVideoUrl { get; set; } = string.Empty;
        [SupplyParameterFromForm]
        private string newVideoName { get; set; } = string.Empty;
        [SupplyParameterFromForm]
        private string newVideoDescription { get; set; } = string.Empty;
        private EditContext editContext;
        private ValidationMessageStore validationMessageStore;

        protected override void OnInitialized()
        {
            editContext = new EditContext(this);
            validationMessageStore = new ValidationMessageStore(editContext);
        }

        private async Task HandleValidSubmit()
        {
            try
            {
                statusMessage = "Uploading video...";
                StateHasChanged();

                var videoId = await VideoIndexerClient.UploadUrlAsync(newVideoUrl, newVideoName, newVideoDescription);
                statusMessage = $"Video uploaded successfully with ID: {videoId}";

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
