
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PatientSimulatorAPI.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PatientSimulatorAPI.Services
{
    public class AzureSpeechService : ISpeechService
    {
        private readonly SpeechConfig _speechConfig;
        private readonly ILogger<AzureSpeechService> _logger;

        public AzureSpeechService(IConfiguration config, ILogger<AzureSpeechService> logger)
        {
            _logger = logger;
            var key = config["AzureSettings:AzureSpeech:ApiKey"];
            var region = config["AzureSettings:AzureSpeech:Region"];

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(region))
            {
                _logger.LogError("Azure Speech configuration is missing.");
                throw new ApplicationException("Azure Speech configuration is missing.");
            }

            _speechConfig = SpeechConfig.FromSubscription(key, region);
        }
        public async Task<string> RecognizeAsync(Stream audioStream)
        {
            try
            {
                // Use your BinaryAudioStreamReader to wrap the incoming stream.
                var pullStreamCallback = new BinaryAudioStreamReader(audioStream);
                var audioInputStream = AudioInputStream.CreatePullStream(pullStreamCallback);
                var audioConfig = AudioConfig.FromStreamInput(audioInputStream);
                var recognizer = new SpeechRecognizer(_speechConfig, audioConfig);

                _logger.LogInformation("Starting in-memory speech recognition.");
                var result = await recognizer.RecognizeOnceAsync();

                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    _logger.LogInformation("In-memory speech recognition succeeded.");
                    return result.Text;
                }
                else
                {
                    _logger.LogWarning("In-memory speech recognition failed. Reason: {Reason}", result.Reason);
                    throw new InvalidOperationException($"STT failed: {result.Reason}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during in-memory speech recognition.");
                throw;
            }
            // No file deletion needed since we're not creating temporary files.
        }


        public async Task<byte[]> SynthesizeAsync(string text)
        {
            try
            {
                using var synthesizer = new SpeechSynthesizer(_speechConfig, null);
                var result = await synthesizer.SpeakTextAsync(text);
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    _logger.LogInformation("TTS synthesis succeeded.");
                    return result.AudioData;
                }
                _logger.LogWarning("TTS synthesis failed. Reason: {Reason}", result.Reason);
                throw new InvalidOperationException($"TTS failed: {result.Reason}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during TTS synthesis.");
                throw;
            }
        }
    }
}
