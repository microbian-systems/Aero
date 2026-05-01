using TUnit.Core;
using Aero.Social.Twitter.Client.Models;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Models;

public class ExpansionResolverTests
{
    //#region ResolveAuthor Tests
    [Test]
    public async Task ResolveAuthor_WithValidAuthorId_ReturnsUser()
    {
        // Arrange
        var user = new User { Id = "123", Username = "testuser", Name = "Test User" };
        var tweet = new Tweet { Id = "1", Text = "Hello", AuthorId = "123" };
        var includes = new Includes { Users = new List<User> { user } };

        // Act
        var author = tweet.ResolveAuthor(includes);

        // Assert
        Assert.NotNull(author);
        await Assert.That(author.Username).IsEqualTo("testuser");
        await Assert.That(author.Name).IsEqualTo("Test User");
    }

    [Test]
    public void ResolveAuthor_WithNullTweet_ReturnsNull()
    {
        // Arrange
        Tweet? tweet = null;
        var includes = new Includes { Users = new List<User>() };

        // Act
        var author = tweet.ResolveAuthor(includes);

        // Assert
        Assert.Null(author);
    }

    [Test]
    public void ResolveAuthor_WithNullAuthorId_ReturnsNull()
    {
        // Arrange
        var tweet = new Tweet { Id = "1", Text = "Hello", AuthorId = null };
        var includes = new Includes { Users = new List<User> { new User { Id = "123" } } };

        // Act
        var author = tweet.ResolveAuthor(includes);

        // Assert
        Assert.Null(author);
    }

    [Test]
    public void ResolveAuthor_WithNullIncludes_ReturnsNull()
    {
        // Arrange
        var tweet = new Tweet { Id = "1", Text = "Hello", AuthorId = "123" };
        Includes? includes = null;

        // Act
        var author = tweet.ResolveAuthor(includes);

        // Assert
        Assert.Null(author);
    }

    [Test]
    public void ResolveAuthor_WithEmptyUsersList_ReturnsNull()
    {
        // Arrange
        var tweet = new Tweet { Id = "1", Text = "Hello", AuthorId = "123" };
        var includes = new Includes { Users = new List<User>() };

        // Act
        var author = tweet.ResolveAuthor(includes);

        // Assert
        Assert.Null(author);
    }

    [Test]
    public void ResolveAuthor_WithUserNotFound_ReturnsNull()
    {
        // Arrange
        var tweet = new Tweet { Id = "1", Text = "Hello", AuthorId = "123" };
        var includes = new Includes { Users = new List<User> { new User { Id = "456", Username = "other" } } };

        // Act
        var author = tweet.ResolveAuthor(includes);

        // Assert
        Assert.Null(author);
    }

    //#endregion

    //#region ResolveUser Tests

    [Test]
    public async Task ResolveUser_WithValidUserId_ReturnsUser()
    {
        // Arrange
        var includes = new Includes { Users = new List<User> { new User { Id = "123", Username = "testuser" } } };

        // Act
        var user = includes.ResolveUser("123");

        // Assert
        Assert.NotNull(user);
        await Assert.That(user.Username).IsEqualTo("testuser");
    }

    [Test]
    public void ResolveUser_WithNullIncludes_ReturnsNull()
    {
        // Arrange
        Includes? includes = null;

        // Act
        var user = includes.ResolveUser("123");

        // Assert
        Assert.Null(user);
    }

    [Test]
    public void ResolveUser_WithNullUserId_ReturnsNull()
    {
        // Arrange
        var includes = new Includes { Users = new List<User> { new User { Id = "123" } } };

        // Act
        var user = includes.ResolveUser(null);

        // Assert
        Assert.Null(user);
    }

    [Test]
    public void ResolveUser_WithEmptyUserId_ReturnsNull()
    {
        // Arrange
        var includes = new Includes { Users = new List<User> { new User { Id = "123" } } };

        // Act
        var user = includes.ResolveUser("");

        // Assert
        Assert.Null(user);
    }

    //#endregion

    //#region ResolveTweet Tests

    [Test]
    public async Task ResolveTweet_WithValidTweetId_ReturnsTweet()
    {
        // Arrange
        var includes = new Includes { Tweets = new List<Tweet> { new Tweet { Id = "456", Text = "Original tweet" } } };

        // Act
        var tweet = includes.ResolveTweet("456");

        // Assert
        Assert.NotNull(tweet);
        await Assert.That(tweet.Text).IsEqualTo("Original tweet");
    }

    [Test]
    public void ResolveTweet_WithNullIncludes_ReturnsNull()
    {
        // Arrange
        Includes? includes = null;

        // Act
        var tweet = includes.ResolveTweet("456");

        // Assert
        Assert.Null(tweet);
    }

    [Test]
    public void ResolveTweet_WithTweetNotFound_ReturnsNull()
    {
        // Arrange
        var includes = new Includes { Tweets = new List<Tweet> { new Tweet { Id = "789" } } };

        // Act
        var tweet = includes.ResolveTweet("456");

        // Assert
        Assert.Null(tweet);
    }

    //#endregion

    //#region ResolveMedia (Single) Tests

    [Test]
    public async Task ResolveMedia_WithValidMediaKey_ReturnsMedia()
    {
        // Arrange
        var includes = new Includes { Media = new List<Media> { new Media { MediaKey = "media_1", Type = "photo" } } };

        // Act
        var media = includes.ResolveMedia("media_1");

        // Assert
        Assert.NotNull(media);
        await Assert.That(media.Type).IsEqualTo("photo");
    }

    [Test]
    public void ResolveMedia_WithNullMediaKey_ReturnsNull()
    {
        // Arrange
        var includes = new Includes { Media = new List<Media> { new Media { MediaKey = "media_1" } } };

        // Act
        var media = includes.ResolveMedia((string?)null);

        // Assert
        Assert.Null(media);
    }

    [Test]
    public void ResolveMedia_WithMediaNotFound_ReturnsNull()
    {
        // Arrange
        var includes = new Includes { Media = new List<Media> { new Media { MediaKey = "media_1" } } };

        // Act
        var media = includes.ResolveMedia("media_2");

        // Assert
        Assert.Null(media);
    }

    //#endregion

    //#region ResolveMedia (Multiple) Tests

    [Test]
    public async Task ResolveMedia_WithMultipleKeys_ReturnsMatchingMedia()
    {
        // Arrange
        var includes = new Includes
        {
            Media = new List<Media>
            {
                new Media { MediaKey = "media_1", Type = "photo" },
                new Media { MediaKey = "media_2", Type = "video" },
                new Media { MediaKey = "media_3", Type = "gif" }
            }
        };
        IEnumerable<string> keys = new[] { "media_1", "media_3" };

        // Act
        var media = includes.ResolveMedia(keys);

        // Assert
        await Assert.That(media.Count).IsEqualTo(2);
        await Assert.That(media).Any(m => m.MediaKey == "media_1");
        await Assert.That(media).Any(m => m.MediaKey == "media_3");
    }

    [Test]
    public async Task ResolveMedia_WithEmptyKeys_ReturnsEmptyList()
    {
        // Arrange
        var includes = new Includes { Media = new List<Media> { new Media { MediaKey = "media_1" } } };

        // Act
        var media = includes.ResolveMedia(new List<string>());

        // Assert
        await Assert.That(media).IsEmpty();
    }

    [Test]
    public async Task ResolveMedia_WithNullKeys_ReturnsEmptyList()
    {
        // Arrange
        var includes = new Includes { Media = new List<Media> { new Media { MediaKey = "media_1" } } };

        // Act
        var media = includes.ResolveMedia((IEnumerable<string>?)null);

        // Assert
        await Assert.That(media).IsEmpty();
    }

    [Test]
    public async Task ResolveMedia_WithPartialMatch_ReturnsMatchedOnly()
    {
        // Arrange
        var includes = new Includes
        {
            Media = new List<Media>
            {
                new Media { MediaKey = "media_1" },
                new Media { MediaKey = "media_2" }
            }
        };
        IEnumerable<string> keys = new[] { "media_1", "media_999" };

        // Act
        var media = includes.ResolveMedia(keys);

        // Assert
        await Assert.That(media).HasSingleItem();
        await Assert.That(media[0].MediaKey).IsEqualTo("media_1");
    }

    //#endregion

    //#region ResolveUsersByUsername Tests

    [Test]
    public async Task ResolveUsersByUsername_WithValidUsernames_ReturnsUsers()
    {
        // Arrange
        var includes = new Includes
        {
            Users = new List<User>
            {
                new User { Id = "1", Username = "user1" },
                new User { Id = "2", Username = "user2" },
                new User { Id = "3", Username = "user3" }
            }
        };
        var usernames = new[] { "user1", "user3" };

        // Act
        var users = includes.ResolveUsersByUsername(usernames);

        // Assert
        await Assert.That(users.Count).IsEqualTo(2);
        await Assert.That(users).Any(u => u.Username == "user1");
        await Assert.That(users).Any(u => u.Username == "user3");
    }

    [Test]
    public async Task ResolveUsersByUsername_WithNullUsernames_ReturnsEmptyList()
    {
        // Arrange
        var includes = new Includes { Users = new List<User> { new User { Username = "user1" } } };

        // Act
        var users = includes.ResolveUsersByUsername(null);

        // Assert
        await Assert.That(users).IsEmpty();
    }

    [Test]
    public async Task ResolveUsersByUsername_WithNullIncludes_ReturnsEmptyList()
    {
        // Arrange
        Includes? includes = null;

        // Act
        var users = includes.ResolveUsersByUsername(new[] { "user1" });

        // Assert
        await Assert.That(users).IsEmpty();
    }

    [Test]
    public async Task ResolveUsersByUsername_CaseSensitive_MatchesExactCase()
    {
        // Arrange
        var includes = new Includes
        {
            Users = new List<User>
            {
                new User { Username = "User1" },
                new User { Username = "user2" }
            }
        };
        var usernames = new[] { "user1" }; // lowercase

        // Act
        var users = includes.ResolveUsersByUsername(usernames);

        // Assert
        await Assert.That(users).IsEmpty(); // Should not match "User1" due to case sensitivity
    }

    //#endregion

    //#region Integration Tests

    [Test]
    public async Task ExpansionResolver_ResolvesCompleteExpansionScenario()
    {
        // Arrange - simulate a complete response with expansions
        var author = new User { Id = "author_1", Username = "author", Name = "The Author" };
        var mentionedUser = new User { Id = "mentioned_1", Username = "mentioned" };
        var referencedTweet = new Tweet { Id = "ref_1", Text = "Original tweet", AuthorId = "author_1" };
        var media = new Media { MediaKey = "media_1", Type = "photo" };

        var tweet = new Tweet
        {
            Id = "1",
            Text = "Check this out @mentioned",
            AuthorId = "author_1"
        };

        var includes = new Includes
        {
            Users = new List<User> { author, mentionedUser },
            Tweets = new List<Tweet> { referencedTweet },
            Media = new List<Media> { media }
        };

        // Act
        var resolvedAuthor = tweet.ResolveAuthor(includes);
        var resolvedTweet = includes.ResolveTweet("ref_1");
        var resolvedMedia = includes.ResolveMedia("media_1");
        var resolvedUsers = includes.ResolveUsersByUsername(new[] { "author", "mentioned" });

        // Assert
        Assert.NotNull(resolvedAuthor);
        await Assert.That(resolvedAuthor.Username).IsEqualTo("author");
        Assert.NotNull(resolvedTweet);
        await Assert.That(resolvedTweet.Text).IsEqualTo("Original tweet");
        Assert.NotNull(resolvedMedia);
        await Assert.That(resolvedUsers.Count).IsEqualTo(2);
}

    //#endregion
}