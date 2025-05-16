
using System.ComponentModel.DataAnnotations;

namespace PatientSimulatorAPI.DTOs
{
    public class TTSRequest
    {
        [Required]
        public required string Text { get; set; }
    }
}
