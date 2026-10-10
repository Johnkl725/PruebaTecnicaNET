using System.Net.Mail;

namespace Domain;

public sealed class User
{
    private readonly List<Role> _roles = [];

    private User() { }

    public User(Guid id, string name, string email, string passwordHash, bool isActive, IEnumerable<Role> roles)
    {
        if (id == Guid.Empty) throw new DomainValidationException("El identificador es obligatorio.");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new DomainValidationException("El hash es obligatorio.");
        Id = id;
        Update(name, email, isActive);
        PasswordHash = passwordHash;
        _roles.AddRange(roles.DistinctBy(role => role.Id));
        if (_roles.Count == 0) throw new DomainValidationException("Un usuario debe tener al menos un rol.");
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    public static string NormalizeEmail(string email)
    {
        var normalized = email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > 254 ||
            !MailAddress.TryCreate(normalized, out var address) || address.Address != normalized ||
            !normalized.Contains('@'))
            throw new DomainValidationException("El email no es válido.");
        return normalized;
    }

    public void Update(string name, string email, bool isActive)
    {
        var trimmedName = name?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName) || trimmedName.Length > 100)
            throw new DomainValidationException("El nombre debe tener entre 1 y 100 caracteres.");
        var normalizedEmail = NormalizeEmail(email);
        Name = trimmedName;
        Email = normalizedEmail;
        IsActive = isActive;
    }
}
