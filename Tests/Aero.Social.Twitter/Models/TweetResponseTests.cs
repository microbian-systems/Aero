using TUnit.Core;
using System.Text.Json;
using Aero.Social.Twitter.Client.Models;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Models;

public class TweetResponseTests
{
    [Test]
    public async Task TweetResponse_Deserialization_WithSingleTweet_PopulatesCorrectly()
    {
        // Arrange
        var json = @"{
                ""data"": [
                    {
                        ""id"": ""1234567890"",
                        ""text"": ""Hello, World!"",
                        ""created_at"": ""2020-01-01T00:00:00.000Z"",
                        ""author_id"": ""9876543210""
                    }
                ],
                ""meta"": {
                    ""result_count"": 1,
                    ""newest_id"": ""1234567890"",
                    ""oldest_id"": ""1234567890""
                }
            }";

        // Act
        var response = JsonSerializer.Deserialize<TweetResponse>(json);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        await Assert.That(response.Data).HasSingleItem();
        await Assert.That(response.Data[0].Id).IsEqualTo("1234567890");
        await Assert.That(response.Data[0].Text).IsEqualTo("Hello, World!");
        Assert.NotNull(response.Meta);
        await Assert.That(response.Meta.ResultCount).IsEqualTo(1);
    }

    [Test]
    public async Task TweetResponse_Deserialization_WithMultipleTweets_PopulatesCorrectly()
    {
        // Arrange
        var json = @"{
                ""data"": [
                    {
                        ""id"": ""1234567890"",
                        ""text"": ""First tweet"",
                        ""created_at"": ""2020-01-02T00:00:00.000Z""
                    },
                    {
                        ""id"": ""1234567891"",
                        ""text"": ""Second tweet"",
                        ""created_at"": ""2020-01-01T00:00:00.000Z""
                    }
                ],
                ""meta"": {
                    ""result_count"": 2,
                    ""next_token"": ""next_page_token"",
                    ""newest_id"": ""1234567890"",
                    ""oldest_id"": ""1234567891""
                }
            }";

        // Act
        var response = JsonSerializer.Deserialize<TweetResponse>(json);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        await Assert.That(response.Data.Count).IsEqualTo(2);
        Assert.NotNull(response.Meta);
        await Assert.That(response.Meta.NextToken).IsEqualTo("next_page_token");
    }

    [Test]
    public async Task TweetResponse_Deserialization_WithPaginationTokens_PopulatesCorrectly()
    {
        // Arrange
        var json = @"{
                ""data"": [],
                ""meta"": {
                    ""result_count"": 0,
                    ""next_token"": ""b26v89c19zqg8o3f"",
                    ""previous_token"": ""a12v78c18ypf7n2e""
                }
            }";

        // Act
        var response = JsonSerializer.Deserialize<TweetResponse>(json);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Meta);
        await Assert.That(response.Meta.NextToken).IsEqualTo("b26v89c19zqg8o3f");
        await Assert.That(response.Meta.PreviousToken).IsEqualTo("a12v78c18ypf7n2e");
}
}