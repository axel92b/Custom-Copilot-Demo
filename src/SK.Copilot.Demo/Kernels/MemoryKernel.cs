using Microsoft.Extensions.Azure;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using Elastic.Clients.Elasticsearch;

namespace Demo;

#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0011

public class MemoryKernel : KernelBase
{
    private readonly ChatHistory _chatHistory = [];

    public override Kernel CreateKernel(SKConfig config)
    {
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(config.AzureOpenAICompletionDeploymentName, config.AzureOpenAIEndpoint, config.AzureOpenAIKey);

        builder.Plugins.AddFromType<TimePlugin>();
        //builder.Plugins.AddFromType<DynamicMemoryLoaderPlugin>();

        builder.Services.AddSingleton(KernelMemory);

        return builder.Build();
    }

    public override async Task<IKernelMemory?> CreateMemoryAsync(SKConfig config)
    {
        Memory = new KernelMemoryBuilder()
            .WithAzureAISearchMemoryDb(new AzureAISearchConfig() { Endpoint = config.AzureSearchEndpoint, APIKey = config.AzureSearchKey, Auth = AzureAISearchConfig.AuthTypes.APIKey, UseHybridSearch = true })
            .WithAzureOpenAITextGeneration(new AzureOpenAIConfig { APIKey = config.AzureOpenAIKey, Endpoint = config.AzureOpenAIEndpoint, Deployment = config.AzureOpenAICompletionDeploymentName, Auth = AzureOpenAIConfig.AuthTypes.APIKey }, new GPT4oTokenizer())
            .WithAzureOpenAITextEmbeddingGeneration(new AzureOpenAIConfig { APIKey = config.AzureOpenAIKey, Endpoint = config.AzureOpenAIEndpoint, Deployment = config.AzureOpenAIEmbeddingDeploymentName, Auth = AzureOpenAIConfig.AuthTypes.APIKey }, new GPT4oTokenizer())
            .Build();

        //This will upload all files to Azure Search Service and local memory
        //Comment if data is already uploaded
        var docLocation = Path.Combine(config.MemoryFolderBasePath, "Memories");
        var tasks = Directory
            .GetFiles(docLocation)
            .Select(f => new FileInfo(f))
            .Select(async fileInfo =>
            {
                await Memory.DeleteDocumentAsync(fileInfo.Name);
                return await Memory.ImportDocumentAsync(fileInfo.FullName, documentId: fileInfo.Name);
            });

        await Task.WhenAll(tasks);

        return Memory;
    }

    // public override Task<KernelPlugin[]> CreatePluginsAsync(Kernel kernel)
    // {
    //     KernelPlugin[] plugins = [
    //         kernel.CreatePluginFromPromptDirectory("Prompts"),
    //     ];
    //
    //     return Task.FromResult(plugins);
    // }

    protected override async Task<string?> HandlePrompt(Kernel kernel, string userPrompt)
    {
        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var completionService = kernel.GetRequiredService<IChatCompletionService>();

        if (_chatHistory.Count > 10)
        {
            _chatHistory.RemoveRange(0, 5);
        }

        var searchResult = await Memory.AskAsync(userPrompt, minRelevance: 0, index: "default");

        if (!searchResult.NoResult)
        {
            userPrompt = $"Using this: {searchResult.Result} \nAnswer this: {userPrompt}";
        }

        _chatHistory.AddUserMessage(userPrompt);

        var result = await completionService.GetChatMessageContentAsync(_chatHistory, executionSettings: settings, kernel: kernel);
        if (result.Content != null)
        {
            _chatHistory.AddAssistantMessage(result.Content);
        }

        return result.Content;
    }
}