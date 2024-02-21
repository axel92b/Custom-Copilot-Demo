using Dumpify;
using Microsoft.SemanticKernel;

namespace Demo;

public class Basic01Order01 : BaseDemo
{
    public override Kernel CreateKernel(SKConfig config)
    {
        var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(config.AzureOpenAICompletionDeploymentName, config.AzureOpenAIEndpoint, config.AzureOpenAIKey)
            // .AddOpenAIChatCompletion(config.OpenAICompletionModelId, config.OpenAIKey)
            .Build();

        return kernel;
    }

    public override string ScreenPrompt => "What do you want to order?";

    protected override async Task<string?> HandlePrompt(Kernel kernel, string userPrompt)
    {
        var calculatedPrompt = GetPromptVersion3(userPrompt);

        return await kernel.InvokePromptAsync<string>(calculatedPrompt);
    }

    private string GetPromptVersion2(string userPrompt)
    => $"""
            What product is ordered in this request? {userPrompt}
            A product can be Coffee, Burger, Water, Shawarma, Shintzel, Beer.
         """;

    private string GetPromptVersion3(string userPrompt)
        => $"""
             What product is ordered in this request? {userPrompt}
             A product can be Coffee, Burger, Water, Shawarma, Shintzel, Beer.
             If the product is not in the list, return NotAvailable
         """;

}