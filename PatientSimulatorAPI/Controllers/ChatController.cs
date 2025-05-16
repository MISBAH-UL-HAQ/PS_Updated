using Azure;
using Microsoft.AspNetCore.Mvc;
using PatientSimulatorAPI.Interfaces;
using static PatientSimulatorAPI.DTOs.ChatDto;
using Microsoft.Extensions.Logging;
namespace PatientSimulatorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [HttpPost("doctor")]

        public async Task<ActionResult<ChatResponseDto>> PostDoctorMessage([FromBody] ChatRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state received in ChatRequestDto: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }
            try
            {
                var response = await _chatService.ProcessDoctorQuestionAsync(request, cancellationToken);
                _logger.LogInformation("Processed chat request successfully for session {SessionId}", request.SessionId);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing chat request.");
                return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}
