namespace VinhKhanhNarration.Api.Utils;

public class PasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) return false;
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
