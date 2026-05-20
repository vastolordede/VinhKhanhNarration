using Npgsql;
using VinhKhanhNarration.Api.Database;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.DAO;

public class LanguageDAO : GenericCrudDAO<LanguageDTO>
{
    public LanguageDAO(DbConnectionFactory factory) : base(factory) { }

    public LanguageDTO? GetByCode(string code)
    {
        return QuerySingle("SELECT * FROM languages WHERE language_code = @code;", cmd => cmd.Parameters.AddWithValue("@code", code));
    }

    public LanguageDTO? GetDefault()
    {
        return QuerySingle("SELECT * FROM languages WHERE is_default = TRUE LIMIT 1;");
    }

    public bool SetDefault(long languageId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        using var cmd1 = new NpgsqlCommand("UPDATE languages SET is_default = FALSE;", conn, tx);
        cmd1.ExecuteNonQuery();
        using var cmd2 = new NpgsqlCommand("UPDATE languages SET is_default = TRUE, is_active = TRUE WHERE language_id = @id;", conn, tx);
        cmd2.Parameters.AddWithValue("@id", languageId);
        var ok = cmd2.ExecuteNonQuery() > 0;
        tx.Commit();
        return ok;
    }

    public bool IsCodeExists(string code)
    {
        return Exists("SELECT COUNT(1) FROM languages WHERE language_code = @code;", cmd => cmd.Parameters.AddWithValue("@code", code));
    }
}
