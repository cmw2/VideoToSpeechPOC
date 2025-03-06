# Video Summary to Speech POC

## Introduction
This project is a Proof of Concept (POC) for generating a summary of a video and then converting that summary to speech using Azure AI Services. The application allows users to upload a file, generate the summary, and convert the summary to an audio file. The audio file can then be downloaded for further use.

## Sample Code
This repository contains sample code intended for demonstration purposes. It showcases how to integrate Azure Cognitive Services for video summarization and speech synthesis. The code is provided as-is and may require modifications to fit specific use cases or production environments.

## Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Azure Subscription](https://azure.microsoft.com/en-us/free/)
- Azure AI Services:
  - Azure Open AI
  - Azure AI Video Indexer
  - Azure Speech

### Installation
1. Clone the repository:
    ```sh
    git clone https://github.com/cmw2/VideoToSpeechPOC.git
    cd VideoToSpeechPOC
    ```

1. Open the VideoToSpeechPOC.sln file in Visual Studio
1. Open the `appsettings.json` file and update the following sections with your Azure credentials:

    ```json
    {
      // ...existing code...
      "AzureVideoIndexer": {
        "ARMBaseUrl": "https://management.azure.com/",
        "ARMApiVersion": "2022-08-01",
        "TenantId": "YOUR_TENANT_ID",
        "ClientId": "",
        "ClientSecret": "",
        "SubscriptionId": "YOUR_AZURE_SUBSCRIPTION_ID",
        "ResourceGroupName": "YOUR_RESOURCE_GROUP_NAME",
        "AccountName": "YOUR_ACCOUNT_NAME",
        "ApiEndpoint": "https://api.videoindexer.ai"
      },
      "AzureSpeech": {
        "Region": "YOUR_AZURE_SPEECH_REGION",
        "ApiKey": "YOUR_AZURE_SPEECH_API_KEY",
        "VoiceName": "en-US-AriaNeural"
      }
      // ...existing code...
    }
    ```

    For security reasons, it is recommended to use dotnet user-secrets to store sensitive information such as API keys. Use the Visual Studio "Manage User Secrets" functionality (right click the project in Solution Explorer) to edit the secrets json.  Paste in the json from above and enter the secrets.  Remove the elements that don't include secrets.

### Running the Application
1. Run the application in Visual Studio.

### Usage
1. If you don't have any videos uploaded, you can use the "Add Video" options in the menu.
1. Track the progress of file processing from the home page.
1. Select a processed file to view it's details and the text summaries
1. If there aren't any summaries, click the "Request Summary" button to generate a summary.
1. For existing summaires, click the "Generate Speech" button to convert the summary to speech.
1. Listen to the generated speech or use the download link to save the file locally.

## Additional Information
- **Technologies Used**: ASP.NET Core, Blazor (Server), Azure Open AI, Azure AI Video Indexer, Azure Speech Service

## Disclaimer
**This Sample Code is provided for the purpose of illustration only and is not intended to be used in a production environment. THIS SAMPLE CODE AND ANY RELATED INFORMATION ARE PROVIDED 'AS IS' WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.**
