using TUnit.Core;
using System.Collections.Generic;
using FluentAssertions;

namespace Electra.Core.Tests;

/// <summary>
/// Represents a class for SnowflakeTests.
/// </summary>
public class SnowflakeTests
{
        /// <summary>
    /// NewId_ShouldGenerateUniqueIds method.
    /// </summary>
[Test]
    public void NewId_ShouldGenerateUniqueIds()
    {
        // Arrange
        var ids = new HashSet<string>();
        var numberOfIdsToGenerate = 10_000_000;

        // Act
        for (int i = 0; i < numberOfIdsToGenerate; i++)
        {
            var newId = Snowflake.NewId();
            var res = ids.Add(newId);
            res.Should() 
                .BeTrue($"because Duplicate IDs should not exist: {newId}");
        }

        // Assert
        numberOfIdsToGenerate.Should().Be(ids.Count);
}
}