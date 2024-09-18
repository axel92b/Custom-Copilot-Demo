namespace Demo;

public interface IDemo : IAsyncDisposable
{
    Task InitializeAsync(SKConfig config);
    Task<string> RunAsync(SKConfig config, string query);
}