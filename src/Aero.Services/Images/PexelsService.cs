using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Aero.Services.Images;

public sealed class PexelsService : IPexelsService, IDisposable
{
    private readonly HttpClient _http;
    private readonly ILogger<PexelsService> _log;
    private readonly string? _apiKey;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string PhotosBase = "https://api.pexels.com/v1/";
    private const string VideosBase = "https://api.pexels.com/videos/";

    public PexelsService(HttpClient http, ILogger<PexelsService> log)
    {
        _http = http;
        _log = log;
        _apiKey = Environment.GetEnvironmentVariable("PEXELS_API_KEY");

        if (!string.IsNullOrEmpty(_apiKey))
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(_apiKey);
        }
        else
        {
            _log.LogWarning("PEXELS_API_KEY environment variable is not set. Pexels API calls will fail.");
        }
    }

    // ─── Photos ──────────────────────────────────────────────

    public async Task<IReadOnlyList<PexelsPhoto>> SearchPhotosAsync(string query, int count = 5, string? orientation = null, CancellationToken ct = default)
    {
        if (!EnsureApiKey()) return [];

        if (string.IsNullOrWhiteSpace(query))
        {
            _log.LogWarning("Pexels: SearchPhotos skipped — query is null or empty");
            return [];
        }

        var sw = Stopwatch.StartNew();
        var url = $"{PhotosBase}search?query={Uri.EscapeDataString(query)}&per_page={Math.Clamp(count, 1, 80)}";
        if (!string.IsNullOrEmpty(orientation))
            url += $"&orientation={orientation}";

        try
        {
            var result = await _http.GetFromJsonAsync<PexelsSearchResult>(url, JsonOpts, ct);
            sw.Stop();

            _log.LogInformation("Pexels: SearchPhotos(query={Query}, count={Count}) → {Total} results in {Elapsed}ms",
                query, count, result?.TotalResults ?? 0, sw.ElapsedMilliseconds);

            return result?.Photos ?? [];
        }
        catch (Exception ex)
        {
            sw.Stop();
            _log.LogError(ex, "Pexels: SearchPhotos(query={Query}) failed after {Elapsed}ms", query, sw.ElapsedMilliseconds);
            return [];
        }
    }

    public async Task<PexelsPhoto?> GetPhotoByIdAsync(int id, CancellationToken ct = default)
    {
        if (!EnsureApiKey()) return null;

        var sw = Stopwatch.StartNew();

        try
        {
            var photo = await _http.GetFromJsonAsync<PexelsPhoto>($"{PhotosBase}photos/{id}", JsonOpts, ct);
            sw.Stop();
            _log.LogInformation("Pexels: GetPhotoById(id={Id}) → {Found} in {Elapsed}ms",
                id, photo is not null ? "found" : "not found", sw.ElapsedMilliseconds);
            return photo;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _log.LogError(ex, "Pexels: GetPhotoById(id={Id}) failed after {Elapsed}ms", id, sw.ElapsedMilliseconds);
            return null;
        }
    }

    // ─── Videos ──────────────────────────────────────────────

    public async Task<IReadOnlyList<PexelsVideo>> SearchVideosAsync(string query, int count = 5, CancellationToken ct = default)
    {
        if (!EnsureApiKey()) return [];

        if (string.IsNullOrWhiteSpace(query))
        {
            _log.LogWarning("Pexels: SearchVideos skipped — query is null or empty");
            return [];
        }

        var sw = Stopwatch.StartNew();
        var url = $"{VideosBase}search?query={Uri.EscapeDataString(query)}&per_page={Math.Clamp(count, 1, 80)}";

        try
        {
            var result = await _http.GetFromJsonAsync<PexelsVideoSearchResult>(url, JsonOpts, ct);
            sw.Stop();

            _log.LogInformation("Pexels: SearchVideos(query={Query}, count={Count}) → {Total} results in {Elapsed}ms",
                query, count, result?.TotalResults ?? 0, sw.ElapsedMilliseconds);

            return result?.Videos ?? [];
        }
        catch (Exception ex)
        {
            sw.Stop();
            _log.LogError(ex, "Pexels: SearchVideos(query={Query}) failed after {Elapsed}ms", query, sw.ElapsedMilliseconds);
            return [];
        }
    }

    public async Task<PexelsVideo?> GetVideoByIdAsync(int id, CancellationToken ct = default)
    {
        if (!EnsureApiKey()) return null;

        var sw = Stopwatch.StartNew();

        try
        {
            var video = await _http.GetFromJsonAsync<PexelsVideo>($"{VideosBase}videos/{id}", JsonOpts, ct);
            sw.Stop();
            _log.LogInformation("Pexels: GetVideoById(id={Id}) → {Found} in {Elapsed}ms",
                id, video is not null ? "found" : "not found", sw.ElapsedMilliseconds);
            return video;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _log.LogError(ex, "Pexels: GetVideoById(id={Id}) failed after {Elapsed}ms", id, sw.ElapsedMilliseconds);
            return null;
        }
    }

    // ─── Downloads ───────────────────────────────────────────

    public async Task<string> DownloadPhotoAsync(PexelsPhoto photo, string subfolder, string filename, CancellationToken ct = default)
    {
        var url = photo.Src.Large2x ?? photo.Src.Large ?? photo.Src.Original;
        var ext = Path.GetExtension(new Uri(url).AbsolutePath);
        if (string.IsNullOrEmpty(ext)) ext = ".jpg";

        return await DownloadFromUrl(url, subfolder, $"{filename}{ext}", ct);
    }

    public async Task<string> DownloadVideoAsync(PexelsVideo video, string subfolder, string filename, CancellationToken ct = default)
    {
        // Pick the highest quality HD video file
        var bestFile = video.VideoFiles
            .Where(f => f.Quality == "hd" || f.Quality == "uhd")
            .OrderByDescending(f => f.Width * f.Height)
            .FirstOrDefault()
            ?? video.VideoFiles
                .OrderByDescending(f => f.Width * f.Height)
                .FirstOrDefault();

        if (bestFile is null)
        {
            _log.LogWarning("Pexels: No video files found for video {VideoId}", video.Id);
            return string.Empty;
        }

        var ext = Path.GetExtension(new Uri(bestFile.Link).AbsolutePath);
        if (string.IsNullOrEmpty(ext)) ext = ".mp4";

        return await DownloadFromUrl(bestFile.Link, subfolder, $"{filename}{ext}", ct);
    }

    private async Task<string> DownloadFromUrl(string url, string subfolder, string filename, CancellationToken ct)
    {
        // Ensure wwwroot/media/ exists relative to current directory
        var mediaDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", subfolder);
        Directory.CreateDirectory(mediaDir);

        var filePath = Path.Combine(mediaDir, filename);

        // Skip if already downloaded
        if (File.Exists(filePath))
        {
            _log.LogInformation("Pexels: File already exists, skipping download: {Path}", filePath);
            return $"/media/{subfolder}/{filename}";
        }

        var sw = Stopwatch.StartNew();

        try
        {
            var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            await stream.CopyToAsync(fileStream, ct);

            sw.Stop();
            var fileSize = new FileInfo(filePath).Length;
            _log.LogInformation("Pexels: Downloaded {Url} → {Path} ({Size} bytes) in {Elapsed}ms",
                url, filePath, fileSize, sw.ElapsedMilliseconds);

            return $"/media/{subfolder}/{filename}";
        }
        catch (Exception ex)
        {
            sw.Stop();
            _log.LogError(ex, "Pexels: Download failed for {Url} after {Elapsed}ms", url, sw.ElapsedMilliseconds);

            // Clean up partial file
            if (File.Exists(filePath)) File.Delete(filePath);
            return string.Empty;
        }
    }

    private bool EnsureApiKey()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _log.LogWarning("PEXELS_API_KEY not configured. Skipping Pexels API call.");
            return false;
        }
        return true;
    }

    public void Dispose() => _http.Dispose();
}
