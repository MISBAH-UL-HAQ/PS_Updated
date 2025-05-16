using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using PatientSimulatorAPI.Interfaces;
using Microsoft.Extensions.Logging;
namespace PatientSimulatorAPI.Services

{
    public class AzureOpenAIService : IOpenAIService
    {
        private readonly ChatClient _chatClient;
        private readonly ILogger<AzureOpenAIService> _logger;

        public AzureOpenAIService(IConfiguration config, ILogger<AzureOpenAIService> logger)
        {
            var endpoint = new Uri(config["AzureOpenAI:Endpoint"]);
            var credential = new AzureKeyCredential(config["AzureOpenAI:ApiKey"]);
            _chatClient = new AzureOpenAIClient(endpoint, credential)
                          .GetChatClient(config["AzureOpenAI:Deployment"]);
            _logger = logger;
        }

        public async Task<string> GetPatientResponseAsync(IEnumerable<ChatMessage> history, ChatCompletionOptions options, CancellationToken cancellationToken = default)
        {
            try
            {

                var result = await _chatClient.CompleteChatAsync(history, options, cancellationToken);

                if (result.Value.Content.Count > 0)
                {
                    _logger.LogInformation("Received a valid response from OpenAI.");
                    return result.Value.Content[0].Text.Trim();
                }
                _logger.LogWarning("OpenAI returned no content in the response.");
                throw new Exception("No response content received from OpenAI.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving response from OpenAI.");
                throw;
            }
        }
    }
}