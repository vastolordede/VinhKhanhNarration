using Npgsql;
using VinhKhanhNarration.Api.Database;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.DAO;

public class AdminUserDAO : GenericCrudDAO<AdminUserDTO>
{
    public AdminUserDAO(DbConnectionFactory factory) : base(factory) { }

    public AdminUserDTO? GetByEmail(string email)
    {
        return QuerySingle("SELECT * FROM admin_users WHERE email = @email;", cmd => cmd.Parameters.AddWithValue("@email", email));
    }

    public bool IsEmailExists(string email)
    {
        return Exists("SELECT COUNT(1) FROM admin_users WHERE email = @email;", cmd => cmd.Parameters.AddWithValue("@email", email));
    }

    public bool UpdatePassword(long adminId, string passwordHash)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand("UPDATE admin_users SET password_hash = @hash WHERE admin_id = @id;", conn);
        cmd.Parameters.AddWithValue("@hash", passwordHash);
        cmd.Parameters.AddWithValue("@id", adminId);
        return cmd.ExecuteNonQuery() > 0;
    }
}
