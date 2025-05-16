using Microsoft.AspNetCore.Http;
namespace PatientSimulatorAPI.DTOs

{
    public class FileUploadDto
    {
        public IFormFile AudioFile { get; set; }
    }
}
