using PatientSimulatorAPI.Models;

namespace PatientSimulatorAPI.Interfaces
{
    public interface IPatientPromptRepository
    {
        //Task<PatientPrompt> GetPatientPromptByIdAsync(int id);
        Task<IEnumerable<PatientPrompt>> GetAllAsync();
        Task<PatientPrompt?> GetByIdAsync(int id);
        Task<PatientPrompt?> GetPatientPromptByIdAsync(int selectedPromptId);
    }
}
