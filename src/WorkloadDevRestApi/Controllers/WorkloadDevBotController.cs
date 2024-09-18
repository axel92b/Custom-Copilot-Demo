using Demo;
using Microsoft.AspNetCore.Mvc;

namespace WorkloadDevRestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkloadDevBotController : ControllerBase
    {
        public static MemoryKernel kernel { get; set; }

        private readonly ILogger<WorkloadDevBotController> _logger;

        SKConfig config = new SKConfig
        {
            AzureOpenAICompletionDeploymentName = "gpt4o",
            AzureOpenAIEmbeddingDeploymentName = "text-embedding-ada-002",
            AzureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_APIKEY")!,
            AzureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!,

            AzureSearchEndpoint = Environment.GetEnvironmentVariable("AZURE_SEARCH_SERVICE_ENDPOINT")!,
            AzureSearchKey = Environment.GetEnvironmentVariable("AZURE_SEARCH_SERVICE_APIKEY")!,
            MemoryFolderBasePath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Debug", "net8.0")
        };

        public WorkloadDevBotController(ILogger<WorkloadDevBotController> logger)
        {
            InitKernel().Wait();
            _logger = logger;
        }

        private async Task InitKernel()
        {
            if (kernel == null)
            {
                kernel = new MemoryKernel();
                await kernel.InitializeAsync(config);
            }
        }

        [HttpPost(Name = "PostPrompt")]
        public async Task<string> PostAsync([FromBody] RequestData data)
        {
            var response = await kernel.RunAsync(config, data.Prompt);
            return response;
        }

        public class RequestData
        {
            public string Prompt { get; set; }
            public string ConversationId { get; set; }
        }
    }
}
