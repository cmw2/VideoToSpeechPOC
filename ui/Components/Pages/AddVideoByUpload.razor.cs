using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using VideoIndexer;
using VideoIndexer.Model;

namespace VideoToSpeechPOC.Components.Pages
{
    public partial class AddVideoByUpload
    {
        [Inject]
        private VideoIndexerClient VideoIndexerClient { get; set; } = default!;

        [Inject]
        private ILogger<AddVideoByUpload> Logger { get; set; } = default!;

        private string statusMessage = string.Empty;
        [SupplyParameterFromForm]
        private string newVideoName { get; set; } = string.Empty;
        [SupplyParameterFromForm]
        private string newVideoDescription { get; set; } = string.Empty;
        [SupplyParameterFromForm]
        private IBrowserFile? selectedFile { get; set; } = null;
        private EditContext editContext;
        private ValidationMessageStore validationMessageStore;

        protected override void OnInitialized()
        {
            editContext = new EditContext(this);
            validationMessageStore = new ValidationMessageStore(editContext);
        }

        private async Task HandleFileSelected(InputFileChangeEventArgs e)
        {
            selectedFile = e.File;
            statusMessage = $"Selected file: {selectedFile.Name}";
            StateHasChanged();
        }

        private async Task HandleValidSubmit()
        {
            try
            {
                statusMessage = "Uploading video...";
                StateHasChanged();

                if (selectedFile != null)
                {
                    var videoId = await VideoIndexerClient.FileUploadAsync(newVideoName, selectedFile.Name, selectedFile.OpenReadStream() ,newVideoDescription);
                    statusMessage = $"Video uploaded successfully with ID: {videoId}";
                }

                // Clear the form fields
                newVideoName = string.Empty;
                newVideoDescription = string.Empty;
                selectedFile = null;

                // Notify the EditContext that the validation state has changed
                editContext.NotifyValidationStateChanged();
            }
            catch (Exception ex)
            {
                statusMessage = $"Error uploading video: {ex.Message}";
                Logger.LogError(ex, "Error uploading video");

                // Add validation message
                validationMessageStore.Clear();
                validationMessageStore.Add(() => newVideoName, ex.Message);
                editContext.NotifyValidationStateChanged();
            }

            StateHasChanged();
        }
    }
}
