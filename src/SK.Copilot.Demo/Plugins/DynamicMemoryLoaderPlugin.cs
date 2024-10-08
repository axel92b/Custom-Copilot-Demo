using System.ComponentModel;
using Dumpify;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;

namespace Demo;

public class DynamicMemoryLoaderPlugin
{
    private readonly IKernelMemory _kernelMemory;

    public DynamicMemoryLoaderPlugin(IKernelMemory kernelMemory)
    {
        _kernelMemory = kernelMemory;
    }

    [KernelFunction("get_additional_info"), Description("Contains additional info for any user query or question")]
    public async Task<string> LoadDoc([Description("The full query of the user")] string userQuery)
    {
        $"Loading memory according to {userQuery}".Dump();
        var result = await _kernelMemory.AskAsync(userQuery, minRelevance: 0, index: "default");
        return result.Result;
    }
}