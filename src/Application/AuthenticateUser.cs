using Domain;

namespace Application;

public sealed class AuthenticateUser(IUserRepository users, IPasswordHasher passwords, ITokenGenerator tokens)
    : IAuthenticateUser
{
    public async Task<TokenDto> ExecuteAsync(string email, string password, CancellationToken cancellationToken)
    {
        var normalizedEmail = User.NormalizeEmail(email);
        if (string.IsNullOrWhiteSpace(password) || password.Length > 1024)
            throw new InvalidInputException("La contraseña debe tener entre 1 y 1024 caracteres.");
        var user = await users.FindByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null || !user.IsActive || !passwords.Verify(password, user.PasswordHash))
            throw new InvalidCredentialsException();
        return tokens.Generate(user);
    }
}
