namespace Aero.Services.Images;

// ─── Photo Models ────────────────────────────────────────

/// <summary>
/// Represents a record for PexelsSearchResult.
/// </summary>
public sealed record PexelsSearchResult(
    [property: JsonPropertyName("photos")] IReadOnlyList<PexelsPhoto> Photos,
    [property: JsonPropertyName("total_results")] int TotalResults,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("per_page")] int PerPage
);

/// <summary>
/// Represents a record for PexelsPhoto.
/// </summary>
public sealed record PexelsPhoto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("src")] PexelsSrc Src,
    [property: JsonPropertyName("alt")] string Alt,
    [property: JsonPropertyName("photographer")] string Photographer
);

/// <summary>
/// Represents a record for PexelsSrc.
/// </summary>
public sealed record PexelsSrc(
    [property: JsonPropertyName("original")] string Original,
    [property: JsonPropertyName("large")] string Large,
    [property: JsonPropertyName("large2x")] string Large2x,
    [property: JsonPropertyName("medium")] string Medium,
    [property: JsonPropertyName("small")] string Small,
    [property: JsonPropertyName("tiny")] string Tiny
);

// ─── Video Models ────────────────────────────────────────

/// <summary>
/// Represents a record for PexelsVideoSearchResult.
/// </summary>
public sealed record PexelsVideoSearchResult(
    [property: JsonPropertyName("videos")] IReadOnlyList<PexelsVideo> Videos,
    [property: JsonPropertyName("total_results")] int TotalResults,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("per_page")] int PerPage
);

/// <summary>
/// Represents a record for PexelsVideo.
/// </summary>
public sealed record PexelsVideo(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("image")] string Image,
    [property: JsonPropertyName("video_files")] IReadOnlyList<PexelsVideoFile> VideoFiles,
    [property: JsonPropertyName("user")] PexelsVideoUser? User
);

/// <summary>
/// Represents a record for PexelsVideoFile.
/// </summary>
public sealed record PexelsVideoFile(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("quality")] string Quality,
    [property: JsonPropertyName("link")] string Link,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height
);

/// <summary>
/// Represents a record for PexelsVideoUser.
/// </summary>
public sealed record PexelsVideoUser(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("url")] string Url
);
