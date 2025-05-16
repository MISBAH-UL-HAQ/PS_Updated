using PatientSimulatorAPI.Models;

namespace PatientSimulatorAPI.Interfaces
{
    public interface ISpeechService
    {
        Task<string> RecognizeAsync(Stream audioStream);
        Task<byte[]> SynthesizeAsync(string text);
    
    }
}
