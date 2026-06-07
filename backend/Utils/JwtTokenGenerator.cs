using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.Utils;

public class JwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public int AccessTokenMinutes =>
        int.TryParse(_configuration["Jwt:AccessTokenMinutes"], out var minutes)
            ? minutes
            : 30;

    public int RefreshTokenDays =>
        int.TryParse(_configuration["Jwt:RefreshTokenDays"], out var days)
            ? days
            : 7;

    public DateTime AccessTokenExpiresAtUtc =>
        DateTime.UtcNow.AddMinutes(AccessTokenMinutes);

    public DateTime RefreshTokenExpiresAtUtc =>
        DateTime.UtcNow.AddDays(RefreshTokenDays);

    public string GenerateAccessToken(AdminUserDTO user, DateTime expiresAtUtc)
    {
        var secretKey = _configuration["Jwt:SecretKey"];

        if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
        {
            throw new InvalidOperationException("Jwt:SecretKey must be at least 32 characters.");
        }

        var issuer = _configuration["Jwt:Issuer"] ?? "VinhKhanhNarration";
        var audience = _configuration["Jwt:Audience"] ?? "VinhKhanhNarrationClient";

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.AdminId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.FullName),
            new Claim(ClaimTypes.NameIdentifier, user.AdminId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("adminId", user.AdminId.ToString()),
            new Claim("role", user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAtUtc,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(bytes);
    }
}