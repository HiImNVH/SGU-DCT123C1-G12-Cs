namespace TravelGuide.Services
{
    public interface INarrationService
    {
        Task SpeakAsync(string text, string languageCode);
        Task StopAsync();
        Task PauseAsync();
        Task ResumeAsync();
        bool IsSpeaking { get; }
    }
}