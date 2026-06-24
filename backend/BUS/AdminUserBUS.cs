using VinhKhanhNarration.Api.BUS.Interfaces;
using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO;
using VinhKhanhNarration.Api.Utils;

namespace VinhKhanhNarration.Api.BUS;

public class AdminUserBUS : ICrudBUS<AdminUserDTO, long>
{
    private readonly AdminUserDAO _dao;
    private readonly PasswordHasher _hasher;
    private readonly AdminRefreshTokenDAO _refreshTokenDAO;
private readonly JwtTokenGenerator _jwtTokenGenerator;
    private static readonly HashSet<string> AllowedRoles = new() { "Admin", "ContentManager", "Translator", "Reviewer" };

    public AdminUserBUS(
    AdminUserDAO dao,
    AdminRefreshTokenDAO refreshTokenDAO,
    PasswordHasher hasher,
    JwtTokenGenerator jwtTokenGenerator)
{
    _dao = dao;
    _refreshTokenDAO = refreshTokenDAO;
    _hasher = hasher;
    _jwtTokenGenerator = jwtTokenGenerator;
}

    public long Create(AdminUserDTO dto)
    {
        ValidateBeforeCreate(dto);
        dto.PasswordHash = _hasher.HashPassword(dto.PasswordHash);
        dto.IsActive = true;
        return _dao.Insert(dto);
    }

    public bool Update(AdminUserDTO dto)
    {
        ValidateBeforeUpdate(dto);
        var current = _dao.GetById(dto.AdminId) ?? throw new InvalidOperationException("Admin user not found.");
        dto.PasswordHash = current.PasswordHash;
        return _dao.Update(dto);
    }

    public bool Deactivate(long id) => _dao.SoftDelete(id);
    public bool Restore(long id) => _dao.Restore(id);
    public AdminUserDTO? GetById(long id) => _dao.GetById(id);
    public List<AdminUserDTO> GetAll() => _dao.GetAll();
    public List<AdminUserDTO> GetActive() => _dao.GetActive();
public LoginResponseDTO? Login(string email, string password, string? ipAddress)
{
    var user = _dao.GetByEmail(email);
    if (user == null || !user.IsActive) return null;
    if (!_hasher.VerifyPassword(password, user.PasswordHash)) return null;

    return CreateAuthResponse(user, ipAddress);
}
    public LoginResponseDTO RefreshAccessToken(string refreshToken, string? ipAddress)
{
    if (string.IsNullOrWhiteSpace(refreshToken))
    {
        throw new ArgumentException("Refresh token is required.");
    }

    var tokenHash = _jwtTokenGenerator.HashRefreshToken(refreshToken);
    var storedToken = _refreshTokenDAO.GetByTokenHash(tokenHash);

    if (storedToken == null)
    {
        throw new UnauthorizedAccessException("Invalid refresh token.");
    }

    if (storedToken.IsRevoked)
    {
        _refreshTokenDAO.RevokeAllActiveTokensByAdminId(storedToken.AdminId, ipAddress);
        throw new UnauthorizedAccessException(
            "Refresh token reuse detected. All Admin sessions were revoked.");
    }

    if (storedToken.IsExpired)
    {
        throw new UnauthorizedAccessException("Refresh token has expired.");
    }

    var user = _dao.GetById(storedToken.AdminId);

    if (user == null || !user.IsActive)
    {
        throw new UnauthorizedAccessException("Admin user is inactive or not found.");
    }

    var newRawRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
    var newRefreshTokenHash = _jwtTokenGenerator.HashRefreshToken(newRawRefreshToken);
    var refreshExpiresAt = _jwtTokenGenerator.RefreshTokenExpiresAtUtc;

    _refreshTokenDAO.Insert(new AdminRefreshTokenDTO
    {
        AdminId = user.AdminId,
        TokenHash = newRefreshTokenHash,
        ExpiresAt = refreshExpiresAt,
        CreatedByIp = ipAddress
    });

    _refreshTokenDAO.RevokeToken(tokenHash, ipAddress, newRefreshTokenHash);

    var accessExpiresAt = _jwtTokenGenerator.AccessTokenExpiresAtUtc;
    var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, accessExpiresAt);

    return new LoginResponseDTO
    {
        AccessToken = accessToken,
        RefreshToken = newRawRefreshToken,
        AccessTokenExpiresAt = accessExpiresAt,
        RefreshTokenExpiresAt = refreshExpiresAt,
        Admin = ToAuthUser(user)
    };
}

public void Logout(string refreshToken, string? ipAddress)
{
    if (string.IsNullOrWhiteSpace(refreshToken)) return;

    var tokenHash = _jwtTokenGenerator.HashRefreshToken(refreshToken);
    _refreshTokenDAO.RevokeToken(tokenHash, ipAddress);
}

public void LogoutAll(long adminId, string? ipAddress)
{
    _refreshTokenDAO.RevokeAllActiveTokensByAdminId(adminId, ipAddress);
}

private LoginResponseDTO CreateAuthResponse(AdminUserDTO user, string? ipAddress)
{
    var accessExpiresAt = _jwtTokenGenerator.AccessTokenExpiresAtUtc;
    var refreshExpiresAt = _jwtTokenGenerator.RefreshTokenExpiresAtUtc;

    var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, accessExpiresAt);
    var rawRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
    var refreshTokenHash = _jwtTokenGenerator.HashRefreshToken(rawRefreshToken);

    _refreshTokenDAO.Insert(new AdminRefreshTokenDTO
    {
        AdminId = user.AdminId,
        TokenHash = refreshTokenHash,
        ExpiresAt = refreshExpiresAt,
        CreatedByIp = ipAddress
    });

    return new LoginResponseDTO
    {
        AccessToken = accessToken,
        RefreshToken = rawRefreshToken,
        AccessTokenExpiresAt = accessExpiresAt,
        RefreshTokenExpiresAt = refreshExpiresAt,
        Admin = ToAuthUser(user)
    };
}

private static AdminAuthUserDTO ToAuthUser(AdminUserDTO user)
{
    return new AdminAuthUserDTO
    {
        AdminId = user.AdminId,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role
    };
}

    public bool ChangePassword(
        long adminId,
        string oldPassword,
        string newPassword,
        string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            throw new ArgumentException("New password must contain at least 8 characters.");
        var user = _dao.GetById(adminId)
            ?? throw new InvalidOperationException("Admin user not found.");
        if (!_hasher.VerifyPassword(oldPassword, user.PasswordHash)) return false;
        if (!_dao.UpdatePassword(adminId, _hasher.HashPassword(newPassword))) return false;
        _refreshTokenDAO.RevokeAllActiveTokensByAdminId(adminId, ipAddress);
        return true;
    }

    public bool ValidateRole(string role) => AllowedRoles.Contains(role);

    private void ValidateBeforeCreate(AdminUserDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName)) throw new ArgumentException("FullName is required.");
        if (!ValidationHelper.IsValidEmail(dto.Email)) throw new ArgumentException("Email is invalid.");
        if (!ValidateRole(dto.Role)) throw new ArgumentException("Role is invalid.");
        if (string.IsNullOrWhiteSpace(dto.PasswordHash)) throw new ArgumentException("Password is required.");
        if (_dao.IsEmailExists(dto.Email)) throw new InvalidOperationException("Email already exists.");
    }

    private void ValidateBeforeUpdate(AdminUserDTO dto)
    {
        if (dto.AdminId <= 0) throw new ArgumentException("AdminId is required.");
        if (string.IsNullOrWhiteSpace(dto.FullName)) throw new ArgumentException("FullName is required.");
        if (!ValidationHelper.IsValidEmail(dto.Email)) throw new ArgumentException("Email is invalid.");
        if (!ValidateRole(dto.Role)) throw new ArgumentException("Role is invalid.");
    }
}
