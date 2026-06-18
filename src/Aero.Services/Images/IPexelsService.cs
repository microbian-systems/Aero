namespace Aero.Services.Images;

/// <summary>
/// Service for fetching photos and videos from the Pexels API.
/// Reads the API key from the PEXELS_API_KEY environment variable.
/// Logs request statistics (query, count, latency, results) for observability.
/// </summary>
public interface IPexelsService
{
    /// <summary>
    /// Search for photos matching a query.
    /// </summary>
    /// <param name="query">Search keywords.</param>
    /// <param name="count">Number of results (max 80).</param>
    /// <param name="orientation">Optional orientation filter: "landscape", "portrait", or "square".</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<PexelsPhoto>> SearchPhotosAsync(string query, int count = 5, string? orientation = null, CancellationToken ct = default);

    /// <summary>
    /// Get a single photo by its Pexels ID.
    /// </summary>
    Task<PexelsPhoto?> GetPhotoByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Search for videos matching a query.
    /// </summary>
    Task<IReadOnlyList<PexelsVideo>> SearchVideosAsync(string query, int count = 5, CancellationToken ct = default);

    /// <summary>
    /// Get a single video by its Pexels ID.
    /// </summary>
    Task<PexelsVideo?> GetVideoByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Download a photo from its URL to the specified local path.
    /// Returns the relative path saved.
    /// </summary>
    Task<string> DownloadPhotoAsync(PexelsPhoto photo, string subfolder, string filename, CancellationToken ct = default);

    /// <summary>
    /// Download a video from its URL to the specified local path.
    /// Uses the highest quality HD video file available.
    /// Returns the relative path saved.
    /// </summary>
    Task<string> DownloadVideoAsync(PexelsVideo video, string subfolder, string filename, CancellationToken ct = default);
}
