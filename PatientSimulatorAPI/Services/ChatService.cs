using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using PatientSimulatorAPI.Interfaces;
using static PatientSimulatorAPI.DTOs.ChatDto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PatientSimulatorAPI.Services
{
    public class ChatService : IChatService
    {
        private readonly IMemoryCache _cache;
        private readonly IPatientPromptRepository _promptRepo;
        private readonly IOpenAIService _openAi;
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IMemoryCache cache,
            IPatientPromptRepository promptRepo,
            IOpenAIService openAi,
            ILogger<ChatService> logger)
        {
            _cache = cache;
            _promptRepo = promptRepo;
            _openAi = openAi;
            _logger = logger;

            // Cache expires after 30 minutes of inactivity.
            _cacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30)
            };
        }
       
        private List<ChatMessage> TrimConversationHistory(List<ChatMessage> messages, int maxMessages = 25)
        {
            // Keep the system prompt (assumed to be the first message) and the most recent maxMessages - 1 exchanges.
            if (messages.Count > maxMessages)
            {
                // The first message is reserved (system prompt)
                var trimmed = new List<ChatMessage> { messages[0] };
                // Take the most recent messages, up to (maxMessages - 1) additional messages.
                trimmed.AddRange(messages.Skip(Math.Max(1, messages.Count - (maxMessages - 1))));
                return trimmed;
            }
            return messages;
        }

        public async Task<ChatResponseDto> ProcessDoctorQuestionAsync(ChatRequestDto request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                _logger.LogWarning("Empty doctor message received for session {SessionId}", request.SessionId);
                throw new ArgumentException("Doctor message cannot be empty.");
            }

            // 1. Retrieve conversation history from cache.
            List<ChatMessage> messages;
            if (string.IsNullOrWhiteSpace(request.SessionId) || !_cache.TryGetValue(request.SessionId, out messages))
            {
                // New conversation must include the prompt, age, and gender.
                if (!request.SelectedPromptId.HasValue || !request.Age.HasValue || string.IsNullOrWhiteSpace(request.Gender))
                {
                    _logger.LogWarning("Missing required parameters for a new conversation. PromptId: {PromptId}, Age: {Age}, Gender: {Gender}",
                        request.SelectedPromptId, request.Age, request.Gender);
                    throw new ArgumentException("Prompt ID, Age, and Gender are required for the first message.");
                }

                var prompt = await _promptRepo.GetPatientPromptByIdAsync(request.SelectedPromptId.Value);
                if (prompt == null)
                {
                    _logger.LogWarning("Invalid prompt ID provided: {PromptId}", request.SelectedPromptId);
                    throw new InvalidOperationException("Invalid prompt ID provided.");
                }

                string systemText = $@"{prompt.SystemPrompt}
                    You are a {request.Age}-year-old {request.Gender?.ToLower()} patient.
                    Remember: do not reveal that you are an AI.";

                messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemText)
                };

                // Generate a new session ID if not provided.
                request.SessionId = Guid.NewGuid().ToString();
                _logger.LogInformation("Started new chat session with session ID: {SessionId}", request.SessionId);
            }
            else
            {
                _logger.LogInformation("Continuing existing chat session with session ID: {SessionId}", request.SessionId);
            }

            // 2. Append the doctor's message.
            messages.Add(new UserChatMessage(request.Message));
            _logger.LogInformation("Appended doctor message: {Message}", request.Message);

            // 3. Prepare chat options.
            var options = new ChatCompletionOptions
            {
                Temperature = 0.3f,
                MaxOutputTokenCount = 200,
                TopP = 0.95f,
                FrequencyPenalty = 0,
                PresencePenalty = 0
            };

            messages = TrimConversationHistory(messages);

            // 4. Get response from OpenAI.
            string replyText = string.Empty;
            try
            {
                replyText = await _openAi.GetPatientResponseAsync(messages, options, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting response from OpenAI for session {SessionId}", request.SessionId);
                throw;
            }

            // 5. Append the assistant's reply.
            messages.Add(new AssistantChatMessage(replyText));
            _logger.LogInformation("Appended patient reply.");

            // 6. Cache the updated conversation history.
            _cache.Set(request.SessionId, messages, _cacheOptions);
            _logger.LogInformation("Cached conversation history for session {SessionId}", request.SessionId);

            // 7. Return the response.
            return new ChatResponseDto
            {
                SessionId = request.SessionId,
                PatientReply = replyText
            };
        }
    }
}
