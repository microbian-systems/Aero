using TUnit.Core;
namespace Electra.Crypto.Solana.Tests;

/// <summary>
/// Represents a class for MinimalTest.
/// </summary>
public class MinimalTest
{
        /// <summary>
    /// SimpleTest_ShouldPass method.
    /// </summary>
[Test]
    public void SimpleTest_ShouldPass()
    {
        // Arrange
        var result = 2 + 2;
        
        // Assert
        Assert.Equal(4, result);
    }
}