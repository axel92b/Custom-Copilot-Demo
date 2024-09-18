
using Dumpify;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;

namespace Demo;

public abstract class KernelBase : IDemo
{
    private Kernel? _kernel;
    private Dictionary<string, KernelPlugin> _plugins = [];

    public Dictionary<string, KernelPlugin> Plugins => _plugins;

    protected IKernelMemory? Memory;
    public IKernelMemory KernelMemory => Memory!;

    public KernelBase()
    {
        GetType().Name.Dump(colors: new ColorConfig { PropertyValueColor = "Orange" });
    }

    public async Task InitializeAsync(SKConfig config)
    {
        Memory = await CreateMemoryAsync(config);
        _kernel = CreateKernel(config);
        _plugins = (await CreatePluginsAsync(_kernel!)).ToDictionary(p => p.Name);
    }

    public async Task<string> RunAsync(SKConfig config, string query)
    {
        var result = await HandlePrompt(_kernel!, query);
        return result;
    }

    public abstract Kernel CreateKernel(SKConfig config);
    protected abstract Task<string?> HandlePrompt(Kernel kernel, string userPrompt);

    public virtual Task<KernelPlugin[]> CreatePluginsAsync(Kernel kernel)
        => Task.FromResult<KernelPlugin[]>([]);

    public virtual Task<IKernelMemory?> CreateMemoryAsync(SKConfig config)
        => Task.FromResult<IKernelMemory?>(null);

    public ValueTask DisposeAsync()
        => ValueTask.CompletedTask;
}