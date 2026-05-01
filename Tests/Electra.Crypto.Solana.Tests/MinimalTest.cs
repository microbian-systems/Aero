using TUnit.Core;
namespace Electra.Crypto.Solana.Tests;

public class MinimalTest
{
    [Test]
    public void SimpleTest_ShouldPass()
    {
        // Arrange
        var result = 2 + 2;
        
        // Assert
        Assert.Equal(4, result);
    }
}