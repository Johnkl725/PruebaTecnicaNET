using System.ComponentModel.DataAnnotations;

namespace Api;

public sealed class TokenRequest
{
    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; init; } = string.Empty;
    [Required, MaxLength(1024)]
    public string Password { get; init; } = string.Empty;
}

public sealed class UpdateUserRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;
    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; init; } = string.Empty;
    [Required]
    public bool? IsActive { get; init; }
}
