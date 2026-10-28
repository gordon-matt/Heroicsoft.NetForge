namespace NetForgeApp.Tests;

public class NoteServiceTests
{
    [Fact]
    public async Task SaveAsync_creates_a_note_for_the_current_user()
    {
        var (options, factory) = SqliteDb.Create();
        const string userId = "user-1";

        await using (var context = SqliteDb.CreateContext(options))
        {
            context.Users.Add(SqliteDb.CreateUser(userId));
            await context.SaveChangesAsync();
        }

        var service = new NoteService(
            NullLogger<NoteService>.Instance,
            new TestUserContext(userId),
            new EntityFrameworkRepository<Note>(factory),
            TimeProvider.System);

        var result = await service.SaveAsync(new SaveNoteRequest(null, "Hello", "A note"));

        Assert.Equal(ResultStatus.Ok, result.Status);
        Assert.True(result.IsSuccess);
        Assert.Equal("Hello", result.Value.Title);
        Assert.Equal("A note", result.Value.Body);

        await using var verify = SqliteDb.CreateContext(options);
        var stored = await verify.Set<Note>().SingleAsync();
        Assert.Equal(userId, stored.OwnerUserId);
        Assert.Equal("Hello", stored.Title);
    }
}
