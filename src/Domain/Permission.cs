namespace Domain;

public sealed class Permission
{
    private Permission() { }

    public Permission(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            throw new DomainValidationException("El nombre del permiso no es válido.");
        Id = id;
        Name = name;
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
}
