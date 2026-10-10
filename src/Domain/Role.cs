namespace Domain;

public sealed class Role
{
    private readonly List<Permission> _permissions = [];
    private Role() { }

    public Role(int id, string name, IEnumerable<Permission> permissions)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            throw new DomainValidationException("El nombre del rol no es válido.");
        Id = id;
        Name = name;
        _permissions.AddRange(permissions.DistinctBy(permission => permission.Id));
        if (_permissions.Count == 0) throw new DomainValidationException("Un rol debe tener al menos un permiso.");
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();
}
