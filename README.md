# Video Summary to Speech POC

## Introduction
This project is a Proof of Concept (POC) for generating a summary of a video and then converting that summary to speech using Azure AI Services. The application allows users to upload a file, generate the summary, and convert the summary to an audio file. The audio file can then be downloaded for further use.

## Sample Code
This repository contains sample code intended for demonstration purposes. It showcases how to integrate Azure Cognitive Services for text extraction and speech synthesis. The code is provided as-is and may require modifications to fit specific use cases or production environments.

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

1. Restore the dependencies:
    ```sh
    dotnet restore
    ```

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

    For security reasons, it is recommended to use dotnet user-secrets to store sensitive information such as API keys. Here is how you can set up dotnet user-secrets:
    1. Initialize user secrets in your project:
        ```sh
        dotnet user-secrets init
        ```
    1. Set the secrets using the following commands:
        ```sh
        dotnet user-secrets set "AzureSpeech:ApiKey" "YOUR_AZURE_SPEECH_API_KEY"
        ```
    In this manner the keys won't be checked into your source code.

### Running the Application
1. Build and run the application:
    ```sh
    dotnet run
    ```

2. Open your browser and navigate to `https://localhost:5001` to access the application.

### Usage
1. Upload a file using the "Upload a file" menu item.
2. Track the progress of file processing from the home page.
3. Select a processed file to view it's details and the text summaries
4. ...

## Additional Information
- **Technologies Used**: ASP.NET Core, Blazor (Server), Azure Open AI, Azure AI Video Indexer, Azure Speech Service

## Disclaimer
**This Sample Code is provided for the purpose of illustration only and is not intended to be used in a production environment. THIS SAMPLE CODE AND ANY RELATED INFORMATION ARE PROVIDED 'AS IS' WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.**
