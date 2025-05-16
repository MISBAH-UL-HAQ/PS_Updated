using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PatientSimulatorAPI.DTOs;
using PatientSimulatorAPI.Interfaces;
using System;
using System.Threading.Tasks;

namespace PatientSimulatorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpeechController : ControllerBase
    {
        private readonly ISpeechService _speechService;
        private readonly ILogger<SpeechController> _logger;

        public SpeechController(ISpeechService speechService, ILogger<SpeechController> logger)
        {
            _speechService = speechService;
            _logger = logger;
        }

        /// <summary>
        /// Speech-to-Text (STT) endpoint.
        /// Expects an audio file (WAV) sent as form-data.
        /// </summary>
        [HttpPost("stt")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RecognizeSpeech([FromForm] FileUploadDto dto)
        {
            if (dto.AudioFile == null || dto.AudioFile.Length == 0)
            {
                _logger.LogWarning("Received empty audio file in the request.");
                return BadRequest("No audio file.");
            }

            await using var stream = dto.AudioFile.OpenReadStream();
            try
            {
                var text = await _speechService.RecognizeAsync(stream);
                _logger.LogInformation("Speech recognized successfully.");
                return Ok(new SpeechRecognitionDto { RecognizedText = text });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during speech recognition.");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("tts")]
        [Produces("audio/wav")]
        public async Task<IActionResult> SynthesizeSpeech([FromBody] DTOs.TTSRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Text))
            {
                _logger.LogWarning("TTS request missing text.");
                return BadRequest("Text is required for TTS.");
            }

            try
            {
                var audioData = await _speechService.SynthesizeAsync(request.Text);
                _logger.LogInformation("Text-to-Speech synthesis completed successfully.");
                return File(audioData, "audio/wav");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during TTS synthesis.");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
