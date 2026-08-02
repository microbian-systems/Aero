using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aero.Social.Tests.Infrastructure;

/// <summary>
/// Represents a class for MockResponses.
/// </summary>
public static class MockResponses
{
        /// <summary>
    /// Represents a class for OAuth2.
    /// </summary>
public static class OAuth2
    {
                /// <summary>
        /// TokenResponse method.
        /// </summary>
public static string TokenResponse(string accessToken, string refreshToken = "mock_refresh_token", int expiresIn = 3600)
        {
            return JsonSerializer.Serialize(new
            {
                access_token = accessToken,
                refresh_token = refreshToken,
                expires_in = expiresIn,
                token_type = "Bearer"
            });
        }

                /// <summary>
        /// ErrorResponse method.
        /// </summary>
public static string ErrorResponse(string error, string description)
        {
            return JsonSerializer.Serialize(new
            {
                error,
                error_description = description
            });
        }
    }

        /// <summary>
    /// Represents a class for Facebook.
    /// </summary>
public static class Facebook
    {
                /// <summary>
        /// TokenResponse method.
        /// </summary>
public static string TokenResponse(string accessToken)
        {
            return JsonSerializer.Serialize(new
            {
                access_token = accessToken,
                token_type = "bearer",
                expires_in = 5184000
            });
        }

                /// <summary>
        /// UserInfoResponse method.
        /// </summary>
public static string UserInfoResponse(string id, string name, string picture = "https://example.com/pic.jpg")
        {
            return JsonSerializer.Serialize(new
            {
                id,
                name,
                picture = new
                {
                    data = new { url = picture }
                }
            });
        }

                /// <summary>
        /// PagesResponse method.
        /// </summary>
public static string PagesResponse(params (string Id, string Name, string AccessToken)[] pages)
        {
            return JsonSerializer.Serialize(new
            {
                data = pages.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    access_token = p.AccessToken
                })
            });
        }

                /// <summary>
        /// PermissionsResponse method.
        /// </summary>
public static string PermissionsResponse(params string[] granted)
        {
            return JsonSerializer.Serialize(new
            {
                data = granted.Select(g => new { permission = g, status = "granted" })
            });
        }

                /// <summary>
        /// PostResponse method.
        /// </summary>
public static string PostResponse(string postId)
        {
            return JsonSerializer.Serialize(new { id = postId });
        }

                /// <summary>
        /// ErrorResponse method.
        /// </summary>
public static string ErrorResponse(int code, string message, string type = "OAuthException")
        {
            return JsonSerializer.Serialize(new
            {
                error = new
                {
                    code,
                    message,
                    type
                }
            });
        }
    }

        /// <summary>
    /// Represents a class for LinkedIn.
    /// </summary>
public static class LinkedIn
    {
                /// <summary>
        /// TokenResponse method.
        /// </summary>
public static string TokenResponse(string accessToken)
        {
            return JsonSerializer.Serialize(new
            {
                access_token = accessToken,
                expires_in = 5184000,
                refresh_token = "mock_refresh",
                refresh_token_expires_in = 5184000
            });
        }

                /// <summary>
        /// UserProfileResponse method.
        /// </summary>
public static string UserProfileResponse(string id, string name, string picture = "")
        {
            return JsonSerializer.Serialize(new
            {
                id,
                name = new
                {
                    localized = new { en_US = name }
                },
                profilePicture = new
                {
                    displayImage = picture
                }
            });
        }

                /// <summary>
        /// PostResponse method.
        /// </summary>
public static string PostResponse(string postId, string urn)
        {
            return JsonSerializer.Serialize(new
            {
                id = postId,
                activityUrn = urn
            });
        }
    }

        /// <summary>
    /// Represents a class for Instagram.
    /// </summary>
public static class Instagram
    {
                /// <summary>
        /// TokenResponse method.
        /// </summary>
public static string TokenResponse(string accessToken, string userId)
        {
            return JsonSerializer.Serialize(new
            {
                access_token = accessToken,
                user_id = userId
            });
        }

                /// <summary>
        /// UserInfoResponse method.
        /// </summary>
public static string UserInfoResponse(string id, string username)
        {
            return JsonSerializer.Serialize(new
            {
                id,
                username,
                account_type = "BUSINESS"
            });
        }

                /// <summary>
        /// MediaResponse method.
        /// </summary>
public static string MediaResponse(string containerId)
        {
            return JsonSerializer.Serialize(new { id = containerId });
        }

                /// <summary>
        /// PublishResponse method.
        /// </summary>
public static string PublishResponse(string mediaId)
        {
            return JsonSerializer.Serialize(new { id = mediaId });
        }
    }

        /// <summary>
    /// Represents a class for X.
    /// </summary>
public static class X
    {
                /// <summary>
        /// TokenResponse method.
        /// </summary>
public static string TokenResponse(string token, string tokenSecret)
        {
            return $"oauth_token={token}&oauth_token_secret={tokenSecret}";
        }

                /// <summary>
        /// UserInfoResponse method.
        /// </summary>
public static string UserInfoResponse(long id, string name, string screenName)
        {
            return JsonSerializer.Serialize(new
            {
                id,
                name,
                screen_name = screenName
            });
        }

                /// <summary>
        /// TweetResponse method.
        /// </summary>
public static string TweetResponse(long id, string text)
        {
            return JsonSerializer.Serialize(new
            {
                data = new
                {
                    id = id.ToString(),
                    text
                }
            });
        }
    }

        /// <summary>
    /// Represents a class for Reddit.
    /// </summary>
public static class Reddit
    {
                /// <summary>
        /// TokenResponse method.
        /// </summary>
public static string TokenResponse(string accessToken)
        {
            return JsonSerializer.Serialize(new
            {
                access_token = accessToken,
                token_type = "bearer",
                expires_in = 3600,
                scope = "*"
            });
        }

                /// <summary>
        /// UserInfoResponse method.
        /// </summary>
public static string UserInfoResponse(string name, string id)
        {
            return JsonSerializer.Serialize(new
            {
                name,
                id,
                icon_img = "https://example.com/avatar.png"
            });
        }

                /// <summary>
        /// SubmitResponse method.
        /// </summary>
public static string SubmitResponse(string name, string url)
        {
            return JsonSerializer.Serialize(new
            {
                json = new
                {
                    data = new
                    {
                        name,
                        url
                    }
                }
            });
        }
    }

        /// <summary>
    /// Represents a class for TikTok.
    /// </summary>
public static class TikTok
    {
                /// <summary>
        /// TokenResponse method.
        /// </summary>
public static string TokenResponse(string accessToken, string refreshToken)
        {
            return JsonSerializer.Serialize(new
            {
                access_token = accessToken,
                refresh_token = refreshToken,
                expires_in = 86400,
                token_type = "Bearer"
            });
        }

                /// <summary>
        /// UserInfoResponse method.
        /// </summary>
public static string UserInfoResponse(string openId, string unionId, string displayName)
        {
            return JsonSerializer.Serialize(new
            {
                data = new
                {
                    user = new
                    {
                        open_id = openId,
                        union_id = unionId,
                        display_name = displayName
                    }
                }
            });
        }
    }

        /// <summary>
    /// Represents a class for YouTube.
    /// </summary>
public static class YouTube
    {
                /// <summary>
        /// ChannelResponse method.
        /// </summary>
public static string ChannelResponse(string id, string title)
        {
            return JsonSerializer.Serialize(new
            {
                items = new[]
                {
                    new
                    {
                        id,
                        snippet = new { title },
                        contentDetails = new
                        {
                            relatedPlaylists = new { uploads = $"UU{id}" }
                        }
                    }
                }
            });
        }

                /// <summary>
        /// VideoResponse method.
        /// </summary>
public static string VideoResponse(string id, string title)
        {
            return JsonSerializer.Serialize(new
            {
                id,
                snippet = new { title }
            });
        }
    }

        /// <summary>
    /// Represents a class for Discord.
    /// </summary>
public static class Discord
    {
                /// <summary>
        /// UserInfoResponse method.
        /// </summary>
public static string UserInfoResponse(string id, string username, string discriminator = "0001")
        {
            return JsonSerializer.Serialize(new
            {
                id,
                username,
                discriminator,
                avatar = "abc123"
            });
        }

                /// <summary>
        /// GuildResponse method.
        /// </summary>
public static string GuildResponse(string id, string name)
        {
            return JsonSerializer.Serialize(new
            {
                id,
                name,
                icon = "guild_icon"
            });
        }

                /// <summary>
        /// ChannelResponse method.
        /// </summary>
public static string ChannelResponse(string id, string name)
        {
            return JsonSerializer.Serialize(new { id, name });
        }

                /// <summary>
        /// MessageResponse method.
        /// </summary>
public static string MessageResponse(string id, string content)
        {
            return JsonSerializer.Serialize(new
            {
                id,
                content,
                channel_id = "123456789"
            });
        }
    }

        /// <summary>
    /// Represents a class for Telegram.
    /// </summary>
public static class Telegram
    {
                /// <summary>
        /// MeResponse method.
        /// </summary>
public static string MeResponse(string botUsername)
        {
            return JsonSerializer.Serialize(new
            {
                ok = true,
                result = new
                {
                    id = 123456789,
                    is_bot = true,
                    first_name = "Test Bot",
                    username = botUsername
                }
            });
        }

                /// <summary>
        /// SendMessageResponse method.
        /// </summary>
public static string SendMessageResponse(int messageId, int chatId)
        {
            return JsonSerializer.Serialize(new
            {
                ok = true,
                result = new
                {
                    message_id = messageId,
                    chat = new { id = chatId },
                    date = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }
            });
        }

                /// <summary>
        /// ErrorResponse method.
        /// </summary>
public static string ErrorResponse(int errorCode, string description)
        {
            return JsonSerializer.Serialize(new
            {
                ok = false,
                error_code = errorCode,
                description
            });
        }
    }

        /// <summary>
    /// Represents a class for Bluesky.
    /// </summary>
public static class Bluesky
    {
                /// <summary>
        /// SessionResponse method.
        /// </summary>
public static string SessionResponse(string did, string handle, string accessJwt)
        {
            return JsonSerializer.Serialize(new
            {
                did,
                handle,
                accessJwt
            });
        }

                /// <summary>
        /// ResolveHandleResponse method.
        /// </summary>
public static string ResolveHandleResponse(string did)
        {
            return JsonSerializer.Serialize(new
            {
                did
            });
        }

                /// <summary>
        /// CreateRecordResponse method.
        /// </summary>
public static string CreateRecordResponse(string uri, string cid)
        {
            return JsonSerializer.Serialize(new
            {
                uri,
                cid
            });
        }
    }

        /// <summary>
    /// Represents a class for Mastodon.
    /// </summary>
public static class Mastodon
    {
                /// <summary>
        /// InstanceResponse method.
        /// </summary>
public static string InstanceResponse(string domain)
        {
            return JsonSerializer.Serialize(new
            {
                uri = $"https://{domain}",
                version = "4.0.0"
            });
        }

                /// <summary>
        /// TokenResponse method.
        /// </summary>
public static string TokenResponse(string accessToken)
        {
            return JsonSerializer.Serialize(new
            {
                access_token = accessToken,
                token_type = "Bearer",
                scope = "read write push",
                created_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
        }

                /// <summary>
        /// AccountResponse method.
        /// </summary>
public static string AccountResponse(string id, string username, string displayName)
        {
            return JsonSerializer.Serialize(new
            {
                id,
                username,
                display_name = displayName,
                avatar = "https://example.com/avatar.png"
            });
        }

                /// <summary>
        /// StatusResponse method.
        /// </summary>
public static string StatusResponse(string id, string content)
        {
            return JsonSerializer.Serialize(new
            {
                id,
                content,
                created_at = DateTime.UtcNow.ToString("O")
            });
        }
    }

        /// <summary>
    /// Represents a class for Threads.
    /// </summary>
public static class Threads
    {
                /// <summary>
        /// TokenResponse method.
        /// </summary>
public static string TokenResponse(string accessToken)
        {
            return JsonSerializer.Serialize(new
            {
                access_token = accessToken,
                token_type = "bearer"
            });
        }

                /// <summary>
        /// UserInfoResponse method.
        /// </summary>
public static string UserInfoResponse(string id, string username)
        {
            return JsonSerializer.Serialize(new
            {
                id,
                username,
                threads_profile_picture_url = "https://example.com/pic.jpg"
            });
        }

                /// <summary>
        /// ContainerResponse method.
        /// </summary>
public static string ContainerResponse(string id)
        {
            return JsonSerializer.Serialize(new { id });
        }

                /// <summary>
        /// PublishResponse method.
        /// </summary>
public static string PublishResponse(string id)
        {
            return JsonSerializer.Serialize(new { id });
        }
    }

        /// <summary>
    /// Represents a class for Pinterest.
    /// </summary>
public static class Pinterest
    {
                /// <summary>
        /// TokenResponse method.
        /// </summary>
public static string TokenResponse(string accessToken, string refreshToken)
        {
            return JsonSerializer.Serialize(new
            {
                access_token = accessToken,
                refresh_token = refreshToken,
                token_type = "bearer",
                expires_in = 3600
            });
        }

                /// <summary>
        /// UserResponse method.
        /// </summary>
public static string UserResponse(string id, string username)
        {
            return JsonSerializer.Serialize(new
            {
                id,
                username,
                profile_image = "https://example.com/pic.jpg"
            });
        }

                /// <summary>
        /// BoardResponse method.
        /// </summary>
public static string BoardResponse(string id, string name)
        {
            return JsonSerializer.Serialize(new
            {
                id,
                name
            });
        }

                /// <summary>
        /// PinResponse method.
        /// </summary>
public static string PinResponse(string id)
        {
            return JsonSerializer.Serialize(new { id });
        }
    }
}
