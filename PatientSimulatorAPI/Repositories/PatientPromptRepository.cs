using System.Text.Json;
using PatientSimulatorAPI.Models;
using PatientSimulatorAPI.Interfaces;
namespace PatientSimulatorAPI.Repositories
{

    /// <summary>
    /// Loads PatientPrompts.json from the content root and provides lookup methods.
    /// </summary>
    public class PatientPromptRepository : IPatientPromptRepository
    {
        private readonly string _filePath;

        public PatientPromptRepository(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "PatientPrompts.json");
        }

        public async Task<IEnumerable<PatientPrompt>> GetAllAsync()
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"PatientPrompts.json not found at {_filePath}");

            var json = await File.ReadAllTextAsync(_filePath);
            var collection = JsonSerializer.Deserialize<PatientPromptCollection>(json);

            if (collection?.PatientPrompts == null)
                throw new InvalidOperationException("Failed to parse PatientPrompts.json or no prompts available.");

            return collection.PatientPrompts;
        }

        public async Task<PatientPrompt?> GetByIdAsync(int id)
        {
            var all = await GetAllAsync();
            return all.FirstOrDefault(p => p.Id == id);
        }

        public async Task<PatientPrompt?> GetPatientPromptByIdAsync(int selectedPromptId)
        {
            return await GetByIdAsync(selectedPromptId);
        }
    }
}
