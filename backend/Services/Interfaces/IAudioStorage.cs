namespace VinhKhanhNarration.Api.Services.Interfaces;

public interface IAudioStorage
{
    Task<string> SaveMp3Async(
        Stream audioStream,
        string fileName,
        CancellationToken cancellationToken = default);
}
