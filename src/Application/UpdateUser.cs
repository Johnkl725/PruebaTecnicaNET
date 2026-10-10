using Domain;

namespace Application;

public sealed class UpdateUser(IUserRepository users) : IUpdateUser
{
    public async Task<UserDto> ExecuteAsync(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        if (command.Id == Guid.Empty) throw new InvalidInputException("El identificador no es válido.");
        var email = User.NormalizeEmail(command.Email);
        if (string.IsNullOrWhiteSpace(command.Name) || command.Name.Trim().Length > 100)
            throw new InvalidInputException("El nombre debe tener entre 1 y 100 caracteres.");
        var user = await users.FindByIdAsync(command.Id, cancellationToken)
            ?? throw new UserNotFoundException();
        if (await users.EmailExistsAsync(email, user.Id, cancellationToken))
            throw new EmailConflictException();

        user.Update(command.Name, email, command.IsActive);
        await users.SaveChangesAsync(cancellationToken);
        return new UserDto(user.Id, user.Name, user.Email, user.IsActive);
    }
}
