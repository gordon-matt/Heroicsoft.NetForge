namespace NetForgeApp.Tests.Infrastructure;

public sealed class TestUserContext(string userId) : IUserContextService
{
    public string? GetCurrentUserId() => userId;

    public string? GetCurrentUserName() => "user";

    public bool IsAuthenticated() => true;

    public bool IsAdministrator() => false;

    public IReadOnlyList<string> GetRoleNames() => [];
}
