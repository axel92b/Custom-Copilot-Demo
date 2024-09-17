using Demo;

var config = new SKConfig
{
    AzureOpenAICompletionDeploymentName = "gpt-4o",
    AzureOpenAIEmbeddingDeploymentName = "text-embedding-ada-002",
    AzureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_APIKEY")!,
    AzureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!,

    AzureSearchEndpoint = Environment.GetEnvironmentVariable("AZURE_SEARCH_SERVICE_ENDPOINT")!,
    AzureSearchKey = Environment.GetEnvironmentVariable("AZURE_SEARCH_SERVICE_APIKEY")!
};

await using var memoryKernel = new MemoryKernel();

await memoryKernel.InitializeAsync(config);
await memoryKernel.RunAsync(config);