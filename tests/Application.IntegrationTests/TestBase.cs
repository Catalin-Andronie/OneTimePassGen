using NUnit.Framework;

namespace OneTimePassGen.Application.IntegrationTests;

using static Testing;

public class TestBase
{
    [SetUp]
    public async Task TestSetUp()
    {
        await ResetDatabaseStateAsync();
    }
}