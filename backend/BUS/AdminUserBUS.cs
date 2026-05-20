using VinhKhanhNarration.Api.BUS.Interfaces;
using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO;
using VinhKhanhNarration.Api.Utils;

namespace VinhKhanhNarration.Api.BUS;

public class AdminUserBUS : ICrudBUS<AdminUserDTO, long>
{
    private readonly AdminUserDAO _dao;
    private readonly PasswordHasher _hasher;
    private static readonly HashSet<string> AllowedRoles = new() { "Admin", "ContentManager", "Translator", "Reviewer" };

    public AdminUserBUS(AdminUserDAO dao, PasswordHasher hasher)
    {
        _dao = dao;
        _hasher = hasher;
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

    public LoginResponseDTO? Login(string email, string password)
    {
        var user = _dao.GetByEmail(email);
        if (user == null || !user.IsActive) return null;
        if (!_hasher.VerifyPassword(password, user.PasswordHash)) return null;
        return new LoginResponseDTO { AdminId = user.AdminId, FullName = user.FullName, Email = user.Email, Role = user.Role };
    }

    public bool ChangePassword(long adminId, string oldPassword, string newPassword)
    {
        var user = _dao.GetById(adminId) ?? throw new InvalidOperationException("Admin user not found.");
        if (!_hasher.VerifyPassword(oldPassword, user.PasswordHash)) return false;
        return _dao.UpdatePassword(adminId, _hasher.HashPassword(newPassword));
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
