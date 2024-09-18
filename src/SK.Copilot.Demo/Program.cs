using Demo;
using Dumpify;
using System.Drawing;

var config = new SKConfig
{
    AzureOpenAICompletionDeploymentName = "gpt4o",
    // AzureOpenAICompletionDeploymentName = "gpt-4o",
    AzureOpenAIEmbeddingDeploymentName = "text-embedding-ada-002",
    AzureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_APIKEY")!,
    AzureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!,

    AzureSearchEndpoint = Environment.GetEnvironmentVariable("AZURE_SEARCH_SERVICE_ENDPOINT")!,
    AzureSearchKey = Environment.GetEnvironmentVariable("AZURE_SEARCH_SERVICE_APIKEY")!,
    MemoryFolderBasePath = Directory.GetCurrentDirectory()
};

await using var memoryKernel = new MemoryKernel();

await memoryKernel.InitializeAsync(config);

string ScreenPrompt = "How can I help you?";
while (true)
{
    ScreenPrompt.Dump(colors: new ColorConfig { PropertyValueColor = Color.Aqua });
    var query = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(query))
    {
        continue;
    }

    var result = await memoryKernel.RunAsync(config, query);

    if (result is null)
    {
        Console.WriteLine();
    }
    else
    {
        result.Dump();
    }
}

