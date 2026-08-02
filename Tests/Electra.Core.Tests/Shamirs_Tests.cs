using TUnit.Core;
using Electra.Core.Algorithms;

namespace Electra.Core.Tests;

/// <summary>
/// Represents a class for Shamirs_Tests.
/// </summary>
public class Shamirs_Tests
{
    const string secret = "this is a very long secret with unicode: 你好 пиво";

    private ShamirsSecretManager CreateManager()
    {
        var manager = new ShamirsSecretManager();
        return manager;
    }

        /// <summary>
    /// CreateFragments_WithStringInput_ReturnsFragments method.
    /// </summary>
[Test]
    public void CreateFragments_WithStringInput_ReturnsFragments()
    {
        // Arrange
        var manager = CreateManager();

        // Act
        var fragments = manager.CreateFragments(secret, 3);

        // Assert
        Assert.NotNull(fragments);
        Assert.Equal(3, fragments.Length);
    }

        /// <summary>
    /// CreateFragments_WithByteArrayInput_ReturnsFragments method.
    /// </summary>
[Test]
    public void CreateFragments_WithByteArrayInput_ReturnsFragments()
    {
        // Arrange
        var manager = CreateManager();
        var bytes = Encoding.UTF8.GetBytes(secret);

        // Act
        var fragments = manager.CreateFragments(secret, 3);

        // Assert
        Assert.NotNull(fragments);
        Assert.Equal(3, fragments.Length);
    }

        /// <summary>
    /// ComputeFragments_WithTwoValidFragments_ReturnsOriginalSecret method.
    /// </summary>
[Test]
    public void ComputeFragments_WithTwoValidFragments_ReturnsOriginalSecret()
    {
        // Arrange
        var manager = CreateManager();
        
        var fragments = manager.CreateFragments(secret, 3);

        // Use only 2 out of 3 fragments
        var selectedFragments = new[] { fragments[0], fragments[1] };

        // Act
        var reconstructedSecret = manager.ComputeFragments(selectedFragments);

        // Assert
        var originalSecret = Encoding.UTF8.GetBytes(secret);
        Assert.Equal(originalSecret, reconstructedSecret);
    }

        /// <summary>
    /// Deconstruct_WithByteArray_ReturnsString method.
    /// </summary>
[Test]
    public void Deconstruct_WithByteArray_ReturnsString()
    {
        // Arrange
        var manager = CreateManager();
        var bytes = Encoding.UTF8.GetBytes(secret);

        // Act
        var result = manager.Deconstruct(bytes);

        // Assert
        Assert.Equal(secret, result);
    }

        /// <summary>
    /// Deconstruct_Generic_WithByteArray_ReturnsString method.
    /// </summary>
[Test]
    public void Deconstruct_Generic_WithByteArray_ReturnsString()
    {
        // Arrange
        var manager = CreateManager();
        var bytes = Encoding.UTF8.GetBytes(secret);

        // Act
        var result = manager.Deconstruct(bytes);

        // Assert
        Assert.Equal(secret, result);
    }

        /// <summary>
    /// CreateFragments_WithNullSecret_ThrowsArgumentNullException method.
    /// </summary>
[Test]
    public void CreateFragments_WithNullSecret_ThrowsArgumentNullException()
    {
        // Arrange
        var manager = CreateManager();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => manager.CreateFragments((string)null, 3));
        Assert.Throws<ArgumentNullException>(() => manager.CreateFragments((byte[])null, 3));
    }

        /// <summary>
    /// CreateFragments_WithZeroLengthSecret_ReturnsNull method.
    /// </summary>
[Test]
    public void CreateFragments_WithZeroLengthSecret_ReturnsNull()
    {
        // Arrange
        var manager = CreateManager();
        byte[] bytes = [];

        // Act
        var fragments = manager.CreateFragments(bytes, 3);

        // Assert
        Assert.Null(fragments);
    }

        /// <summary>
    /// CreateFragments_WithLessThanThreeFragments_ThrowsArgumentException method.
    /// </summary>
[Test]
    public void CreateFragments_WithLessThanThreeFragments_ThrowsArgumentException()
    {
        // Arrange
        var manager = CreateManager();
        

        // Act & Assert
        Assert.Throws<ArgumentException>(() => manager.CreateFragments(secret, 2));
        Assert.Throws<ArgumentException>(() => manager.CreateFragments(Encoding.UTF8.GetBytes(secret), 2));
    }

        /// <summary>
    /// Deconstruct_WithByteArray_Extension_ReturnsString method.
    /// </summary>
[Test]
    public void Deconstruct_WithByteArray_Extension_ReturnsString()
    {
        // Arrange
        var manager = new ShamirsSecretManager();
        var bytes = Encoding.UTF8.GetBytes(secret);

        // Act
        var result = manager.Deconstruct(bytes);

        // Assert
        Assert.Equal(secret, result);
    }

        /// <summary>
    /// Deconstruct_Generic_WithByteArray_Extension_ReturnsString method.
    /// </summary>
[Test]
    public void Deconstruct_Generic_WithByteArray_Extension_ReturnsString()
    {
        // Arrange
        var manager = new ShamirsSecretManager();
        var bytes = Encoding.UTF8.GetBytes(secret);

        // Act
        var result = manager.Deconstruct(bytes);

        // Assert
        Assert.Equal(secret, result);
}
}