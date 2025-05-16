using Azure.AI.OpenAI;
using Azure;
using OpenAI.Chat;

namespace PatientSimulatorAPI.Interfaces
{
    public interface IOpenAIService
    {
        //Task<ChatCompletion> GetPatientResponseAsync(IEnumerable<ChatMessage> history, ChatCompletionOptions options);
        //Task<string> GetPatientResponseAsync(IEnumerable<ChatMessage> history, ChatCompletionOptions options);
        Task<string> GetPatientResponseAsync(IEnumerable<ChatMessage> history, ChatCompletionOptions options, CancellationToken cancellationToken = default);
    }
}

