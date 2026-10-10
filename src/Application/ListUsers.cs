namespace Application;

public sealed class ListUsers(IUserRepository users) : IListUsers
{
    public Task<PagedUsers> ExecuteAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        if (page < 1 || pageSize is < 1 or > 100 || (long)(page - 1) * pageSize > int.MaxValue)
            throw new InvalidInputException("page debe ser >= 1 y pageSize entre 1 y 100, sin exceder el desplazamiento máximo.");
        return users.ListAsync(page, pageSize, cancellationToken);
    }
}
