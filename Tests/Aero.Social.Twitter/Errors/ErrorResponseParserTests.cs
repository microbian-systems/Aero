using TUnit.Core;
using Aero.Social.Twitter.Client.Errors;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Errors;

/// <summary>
/// Represents a class for ErrorResponseParserTests.
/// </summary>
public class ErrorResponseParserTests
{
        /// <summary>
    /// ParseErrorResponse_V2Format_ReturnsParsedErrors method.
    /// </summary>
[Test]
    public async Task ParseErrorResponse_V2Format_ReturnsParsedErrors()
    {
        // Arrange
        var json = @"{
                ""errors"": [
                    {
                        ""message"": ""Sorry, that page does not exist"",
                        ""code"": 34,
                        ""field"": ""id""
                    }
                ]
            }";

        // Act
        var errors = ErrorResponseParser.ParseErrorResponse(json);

        // Assert
        await Assert.That(errors).HasSingleItem();
        await Assert.That(errors[0].Code).IsEqualTo(34);
        await Assert.That(errors[0].Message).IsEqualTo("Sorry, that page does not exist");
        await Assert.That(errors[0].Field).IsEqualTo("id");
        Assert.NotNull(errors[0].DocumentationUrl);
    }

        /// <summary>
    /// ParseErrorResponse_V1ErrorFormat_ReturnsParsedError method.
    /// </summary>
[Test]
    public async Task ParseErrorResponse_V1ErrorFormat_ReturnsParsedError()
    {
        // Arrange
        var json = @"{ ""error"": ""Rate limit exceeded"", ""code"": 88 }";

        // Act
        var errors = ErrorResponseParser.ParseErrorResponse(json);

        // Assert
        await Assert.That(errors).HasSingleItem();
        await Assert.That(errors[0].Code).IsEqualTo(88);
        await Assert.That(errors[0].Message).IsEqualTo("Rate limit exceeded");
    }

        /// <summary>
    /// ParseErrorResponse_V1ErrorObjectFormat_ReturnsParsedError method.
    /// </summary>
[Test]
    public async Task ParseErrorResponse_V1ErrorObjectFormat_ReturnsParsedError()
    {
        // Arrange
        var json = @"{ ""errors"": { ""88"": ""Rate limit exceeded"" } }";

        // Act
        var errors = ErrorResponseParser.ParseErrorResponse(json);

        // Assert
        await Assert.That(errors).HasSingleItem();
        await Assert.That(errors[0].Code).IsEqualTo(88);
        await Assert.That(errors[0].Message).IsEqualTo("Rate limit exceeded");
    }

        /// <summary>
    /// ParseErrorResponse_MultipleErrors_ReturnsAllErrors method.
    /// </summary>
[Test]
    public async Task ParseErrorResponse_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var json = @"{
                ""errors"": [
                    {
                        ""message"": ""Sorry, that page does not exist"",
                        ""code"": 34
                    },
                    {
                        ""message"": ""User not found"",
                        ""code"": 50
                    }
                ]
            }";

        // Act
        var errors = ErrorResponseParser.ParseErrorResponse(json);

        // Assert
        await Assert.That(errors.Count).IsEqualTo(2);
        await Assert.That(errors[0].Code).IsEqualTo(34);
        await Assert.That(errors[1].Code).IsEqualTo(50);
    }

        /// <summary>
    /// ParseErrorResponse_V2FormatWithResource_ReturnsResourceInfo method.
    /// </summary>
[Test]
    public async Task ParseErrorResponse_V2FormatWithResource_ReturnsResourceInfo()
    {
        // Arrange
        var json = @"{
                ""errors"": [
                    {
                        ""message"": ""Could not find user"",
                        ""code"": 50,
                        ""resource_type"": ""user"",
                        ""resource_id"": ""12345""
                    }
                ]
            }";

        // Act
        var errors = ErrorResponseParser.ParseErrorResponse(json);

        // Assert
        await Assert.That(errors).HasSingleItem();
        await Assert.That(errors[0].ResourceType).IsEqualTo("user");
        await Assert.That(errors[0].ResourceId).IsEqualTo("12345");
    }

        /// <summary>
    /// ParseErrorResponse_InvalidJson_ReturnsEmptyList method.
    /// </summary>
[Test]
    public async Task ParseErrorResponse_InvalidJson_ReturnsEmptyList()
    {
        // Arrange
        var json = "not valid json";

        // Act
        var errors = ErrorResponseParser.ParseErrorResponse(json);

        // Assert
        await Assert.That(errors).IsEmpty();
    }

        /// <summary>
    /// ParseErrorResponse_NullResponse_ReturnsEmptyList method.
    /// </summary>
[Test]
    public async Task ParseErrorResponse_NullResponse_ReturnsEmptyList()
    {
        // Act
        var errors = ErrorResponseParser.ParseErrorResponse(null);

        // Assert
        await Assert.That(errors).IsEmpty();
    }

        /// <summary>
    /// ParseErrorResponse_EmptyResponse_ReturnsEmptyList method.
    /// </summary>
[Test]
    public async Task ParseErrorResponse_EmptyResponse_ReturnsEmptyList()
    {
        // Act
        var errors = ErrorResponseParser.ParseErrorResponse("");

        // Assert
        await Assert.That(errors).IsEmpty();
    }

        /// <summary>
    /// ParseErrorResponse_NoErrorsField_ReturnsEmptyList method.
    /// </summary>
[Test]
    public async Task ParseErrorResponse_NoErrorsField_ReturnsEmptyList()
    {
        // Arrange
        var json = @"{ ""data"": ""some value"" }";

        // Act
        var errors = ErrorResponseParser.ParseErrorResponse(json);

        // Assert
        await Assert.That(errors).IsEmpty();
    }

        /// <summary>
    /// ParseErrorResponse_V1WithErrorCodeInMessage_ExtractsCode method.
    /// </summary>
[Test]
    public async Task ParseErrorResponse_V1WithErrorCodeInMessage_ExtractsCode()
    {
        // Arrange
        var json = @"{ ""error"": ""Could not authenticate you"" }";

        // Act
        var errors = ErrorResponseParser.ParseErrorResponse(json);

        // Assert
        await Assert.That(errors).HasSingleItem();
        await Assert.That(errors[0].Code).IsEqualTo(32); // Code extracted from message
        await Assert.That(errors[0].Message).IsEqualTo("Could not authenticate you");
    }

        /// <summary>
    /// GetPrimaryErrorMessage_SingleError_ReturnsEnhancedMessage method.
    /// </summary>
[Test]
    public async Task GetPrimaryErrorMessage_SingleError_ReturnsEnhancedMessage()
    {
        // Arrange
        var errors = new List<TwitterError>
        {
            new TwitterError { Code = 88, Message = "Rate limit exceeded" }
        };

        // Act
        var message = ErrorResponseParser.GetPrimaryErrorMessage(errors);

        // Assert
        await Assert.That(message).Contains("Twitter API Error 88");
        await Assert.That(message).Contains("Rate limit exceeded");
    }

        /// <summary>
    /// GetPrimaryErrorMessage_EmptyList_ReturnsDefaultMessage method.
    /// </summary>
[Test]
    public async Task GetPrimaryErrorMessage_EmptyList_ReturnsDefaultMessage()
    {
        // Arrange
        var errors = new List<TwitterError>();

        // Act
        var message = ErrorResponseParser.GetPrimaryErrorMessage(errors);

        // Assert
        await Assert.That(message).IsEqualTo("An unknown error occurred.");
    }

        /// <summary>
    /// GetPrimaryErrorMessage_ErrorWithoutCode_ReturnsMessageOnly method.
    /// </summary>
[Test]
    public async Task GetPrimaryErrorMessage_ErrorWithoutCode_ReturnsMessageOnly()
    {
        // Arrange
        var errors = new List<TwitterError>
        {
            new TwitterError { Code = 0, Message = "Some error without code" }
        };

        // Act
        var message = ErrorResponseParser.GetPrimaryErrorMessage(errors);

        // Assert
        await Assert.That(message).IsEqualTo("Some error without code");
    }

        /// <summary>
    /// BuildComprehensiveErrorMessage_SingleError_ReturnsEnhancedMessage method.
    /// </summary>
[Test]
    public async Task BuildComprehensiveErrorMessage_SingleError_ReturnsEnhancedMessage()
    {
        // Arrange
        var errors = new List<TwitterError>
        {
            new TwitterError { Code = 88, Message = "Rate limit exceeded" }
        };

        // Act
        var message = ErrorResponseParser.BuildComprehensiveErrorMessage(errors);

        // Assert
        await Assert.That(message).Contains("Twitter API Error 88");
    }

        /// <summary>
    /// BuildComprehensiveErrorMessage_MultipleErrors_ReturnsAllErrors method.
    /// </summary>
[Test]
    public async Task BuildComprehensiveErrorMessage_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var errors = new List<TwitterError>
        {
            new TwitterError { Code = 34, Message = "Not found", Field = "id" },
            new TwitterError { Code = 50, Message = "User not found", Field = "username" }
        };

        // Act
        var message = ErrorResponseParser.BuildComprehensiveErrorMessage(errors);

        // Assert
        await Assert.That(message).Contains("Multiple errors occurred (2)");
        await Assert.That(message).Contains("1.");
        await Assert.That(message).Contains("Error 34");
        await Assert.That(message).Contains("2.");
        await Assert.That(message).Contains("Error 50");
        await Assert.That(message).Contains("Field: id");
        await Assert.That(message).Contains("Field: username");
        await Assert.That(message).Contains("developer.twitter.com");
    }

        /// <summary>
    /// BuildComprehensiveErrorMessage_EmptyList_ReturnsDefaultMessage method.
    /// </summary>
[Test]
    public async Task BuildComprehensiveErrorMessage_EmptyList_ReturnsDefaultMessage()
    {
        // Arrange
        var errors = new List<TwitterError>();

        // Act
        var message = ErrorResponseParser.BuildComprehensiveErrorMessage(errors);

        // Assert
        await Assert.That(message).IsEqualTo("An unknown error occurred.");
}
}