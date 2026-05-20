namespace VinhKhanhNarration.Api.DAO.Mapping;

[AttributeUsage(AttributeTargets.Class)]
public sealed class DbTableAttribute : Attribute
{
    public string Name { get; }
    public DbTableAttribute(string name) => Name = name;
}
