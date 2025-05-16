using System.ComponentModel.DataAnnotations;
namespace PatientSimulatorAPI.DTOs
{
    public class ChatDto
    {
        public class ChatRequestDto
        {
            /// <summary>
            /// The GUID for this conversation. 
            /// Leave blank on first call to have server generate one.
            /// </summary>
            public string? SessionId { get; set; }

            /// <summary>
            /// Only on the very first call:
            /// The patient’s condition prompt ID (e.g., 1 for Asthma).
            /// </summary>
           
            public int? SelectedPromptId { get; set; }

            /// <summary>
            /// Only on the very first call: patient’s age.
            /// </summary>
      
            [Range(10, 110, ErrorMessage = "Age must be between 10 and 110.")]
            public int? Age { get; set; }

            /// <summary>
            /// Only on the very first call: patient’s gender ("male"/"female").
            /// </summary>
            
            [RegularExpression("^(male|female)$", ErrorMessage = "Gender must be either 'male' or 'female'.")]
            public string? Gender { get; set; }

            /// <summary>
            /// The doctor’s latest question/text.
            /// </summary>
            [Required(ErrorMessage = "Doctor message is required.")]
            public string Message { get; set; } = string.Empty;
        }

        public class ChatResponseDto
        {
            /// <summary>
            /// The conversation GUID to persist on the client.
            /// </summary>
            public string SessionId { get; set; } = string.Empty;

            /// <summary>
            /// The AI-generated patient reply.
            /// </summary>
            public string PatientReply { get; set; } = string.Empty;
        }
    }
}
