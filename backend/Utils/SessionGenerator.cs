namespace VinhKhanhNarration.Api.Utils;

public class SessionGenerator
{
    public string GenerateGuestSessionId()
    {
        return $"GUEST-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..40];
    }
}
