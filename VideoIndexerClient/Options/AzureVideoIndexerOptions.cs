namespace VideoIndexer.Options;

public class AzureVideoIndexerOptions
{
    public string ARMBaseUrl { get; set; }
    public string ARMApiVersion { get; set; }
    public string TenantId { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string SubscriptionId { get; set; }
    public string ResourceGroupName { get; set; }
    public string AccountName { get; set; }
    public string ApiEndpoint { get; set; }
    
}