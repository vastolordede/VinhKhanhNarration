namespace VinhKhanhNarration.Api.DAO.Mapping;

[AttributeUsage(AttributeTargets.Property)]
public sealed class DbColumnAttribute : Attribute
{
    public string Name { get; }
    public bool IsKey { get; set; }
    public bool IsIdentity { get; set; }
    public bool IgnoreOnInsert { get; set; }
    public bool IgnoreOnUpdate { get; set; }

    public DbColumnAttribute(string name) => Name = name;
}
