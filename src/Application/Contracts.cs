namespace Application;

public static class Permissions
{
    public const string UsersRead = "users.read";
    public const string UsersUpdate = "users.update";
}

public sealed record UserDto(Guid Id, string Name, string Email, bool IsActive);
public sealed record PagedUsers(IReadOnlyList<UserDto> Items, int Page, int PageSize, int TotalCount);
public sealed record TokenDto(string AccessToken, string TokenType, int ExpiresIn);
public sealed record UpdateUserCommand(Guid Id, string Name, string Email, bool IsActive);

public interface IUpdateUser
{
    Task<UserDto> ExecuteAsync(UpdateUserCommand command, CancellationToken cancellationToken);
}

public interface IListUsers
{
    Task<PagedUsers> ExecuteAsync(int page, int pageSize, CancellationToken cancellationToken);
}

public interface IAuthenticateUser
{
    Task<TokenDto> ExecuteAsync(string email, string password, CancellationToken cancellationToken);
}
