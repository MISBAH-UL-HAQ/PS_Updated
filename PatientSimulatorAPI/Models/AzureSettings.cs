namespace PatientSimulatorAPI.Models
{
    public class AzureSpeechSettings
    {
        public string ApiKey { get; set; }
        public string Region { get; set; }
    }

    public class AzureSettings
    {
        public AzureSpeechSettings AzureSpeech { get; set; }
    }
}
