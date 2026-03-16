namespace Aero.RavenDB.Tests;

public class RavenDbEmbeddedSmokeTest : RavenDbTestBase
{
    [Fact]
    public async Task Should_Start_Embedded_RavenDB_And_Store_Document()
    {
        // Arrange
        using var session = DocumentStore.LightweightSession();
        var testDoc = new { Id = "tests/1", Name = "Test" };

        // Act
        session.Store(testDoc);
        await session.SaveChangesAsync();

        // Assert
        using var loadSession = DocumentStore.LightweightSession();
        var loadedDoc = await loadSession.LoadAsync<object>("tests/1");
        loadedDoc.ShouldNotBeNull();
    }
}
