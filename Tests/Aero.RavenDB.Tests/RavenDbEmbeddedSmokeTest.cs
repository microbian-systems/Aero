using TUnit.Core;
namespace Aero.RavenDB.Tests;

/// <summary>
/// Represents a class for RavenDbEmbeddedSmokeTest.
/// </summary>
public class RavenDbEmbeddedSmokeTest : RavenDbTestBase
{
        /// <summary>
    /// Should_Start_Embedded_RavenDB_And_Store_Document method.
    /// </summary>
[Test]
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
