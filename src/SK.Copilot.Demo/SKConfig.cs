namespace Demo;

public class SKConfig
{
    public required string AzureOpenAIEndpoint { get; set; }
    public required string AzureOpenAIKey { get; set; }
    public required string AzureOpenAICompletionDeploymentName { get; set; }
    public required string AzureOpenAIEmbeddingDeploymentName { get; set; }
    public required string AzureSearchEndpoint { get; set; }
    public required string AzureSearchKey { get; set; }

    public required string MemoryFolderBasePath { get; set; }
}