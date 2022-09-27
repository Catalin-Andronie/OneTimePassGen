using FluentAssertions;

using FluentValidation;

using NUnit.Framework;

using OneTimePassGen.Application.UserGeneratedPasswords.Commands.CreateUserGeneratedPassword;
using OneTimePassGen.Domain.Entities;

namespace OneTimePassGen.Application.IntegrationTests.TodoItems.Commands;

using static Testing;

public sealed class CreateUserGeneratedPasswordCommandTests : TestBase
{
    [Test]
    public async Task CreateUserGeneratedPasswordCommand_Should_NotThrown_ValidationException()
    {
        var command = new CreateUserGeneratedPasswordCommand();

        await FluentActions.Invoking(() =>
            SendAsync(command)).Should().NotThrowAsync<ValidationException>();
    }

    [Test]
    public async Task CreateUserGeneratedPasswordCommand_Should_PersistToDatabase()
    {
        CurrentDateTime = new DateTimeOffset(2020, 08, 05, 16, 45, 23, 545, new TimeSpan(2, 0, 0));
        GeneratedPasswordValue = "generated-password";

        var userId = await RunAsDefaultUserAsync();

        var command = new CreateUserGeneratedPasswordCommand();

        var entryId = await SendAsync(command);

        var entry = await FindAsync<UserGeneratedPassword>(entryId);

        entry.Should().NotBeNull();
        entry!.Id.Should().NotBeEmpty();
        entry.Id.Should().Be(entryId);
        entry.UserId.Should().Be(userId);
        entry.Password.Should().Be(GeneratedPasswordValue);
        entry.ExpiersAt.Should().BeExactly(CurrentDateTime.Value.AddSeconds(30), because: "generated passwords should expire after 30 seconds");
        entry.CreatedAt.Should().BeExactly(CurrentDateTime.Value);
    }
}