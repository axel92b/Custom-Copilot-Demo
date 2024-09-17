using Microsoft.Extensions.Azure;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;

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
        builder.Plugins.AddFromType<DynamicMemoryLoaderPlugin>();

        builder.Services.AddSingleton(KernelMemory);

        return builder.Build();
    }

    public override async Task<IKernelMemory?> CreateMemoryAsync(SKConfig config)
    {
        var memory = new KernelMemoryBuilder()
            .WithAzureAISearchMemoryDb(config.AzureSearchEndpoint, config.AzureSearchKey)
            .WithAzureOpenAITextGeneration(new AzureOpenAIConfig { APIKey = config.AzureOpenAIKey, Endpoint = config.AzureOpenAIEndpoint, Deployment = config.AzureOpenAICompletionDeploymentName, Auth = AzureOpenAIConfig.AuthTypes.APIKey })
            .WithAzureOpenAITextEmbeddingGeneration(new AzureOpenAIConfig { APIKey = config.AzureOpenAIKey, Endpoint = config.AzureOpenAIEndpoint, Deployment = config.AzureOpenAIEmbeddingDeploymentName, Auth = AzureOpenAIConfig.AuthTypes.APIKey })
            .Build();

        var docLocation = Path.Combine(Directory.GetCurrentDirectory(), "Memories");
        var tasks = Directory
            .GetFiles(docLocation)
            .Select(f => new FileInfo(f))
            .Select(async fileInfo =>
            {
                await memory.DeleteDocumentAsync(fileInfo.Name);
                return await memory.ImportDocumentAsync(fileInfo.FullName, documentId: fileInfo.Name);
            });

        await Task.WhenAll(tasks);

        return memory;
    }

    // public override Task<KernelPlugin[]> CreatePluginsAsync(Kernel kernel)
    // {
    //     KernelPlugin[] plugins = [
    //         kernel.CreatePluginFromPromptDirectory("Prompts"),
    //     ];
    //
    //     return Task.FromResult(plugins);
    // }

    public override string ScreenPrompt => "What do you want to know about time?";

    protected override async Task<string?> HandlePrompt(Kernel kernel, string userPrompt)
    {
        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
        };

        var completionService = kernel.GetRequiredService<IChatCompletionService>();

        if (_chatHistory.Count > 10)
        {
            _chatHistory.RemoveRange(0, 5);
        }

        _chatHistory.AddUserMessage(userPrompt);

        var stream = completionService.GetStreamingChatMessageContentsAsync(_chatHistory, executionSettings: settings, kernel: kernel);

        var message = new StringBuilder();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        await foreach (var content in stream)
        {
            message.Append(content.Content);
            Console.Write(content.Content);
        }

        Console.ForegroundColor = ConsoleColor.Gray;

        if (message.Length > 0)
        {
            _chatHistory.AddAssistantMessage(message.ToString());
        }

        return null;
    }
}