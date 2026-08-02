namespace Aero.Social.Forem;


        /// <summary>
    /// Represents a class for ArticleCreateRequest.
    /// </summary>
public class ArticleCreateRequest
    {
                /// <summary>
        /// Gets or sets the Article.
        /// </summary>
public ArticleData Article { get; set; } = new();
    }

        /// <summary>
    /// Represents a class for ArticleData.
    /// </summary>
public class ArticleData
    {
                /// <summary>
        /// Gets or sets the Title.
        /// </summary>
public string Title { get; set; } = string.Empty;
                /// <summary>
        /// Gets or sets the Body Markdown.
        /// </summary>
public string BodyMarkdown { get; set; } = string.Empty;
                /// <summary>
        /// Gets or sets the Published.
        /// </summary>
public bool Published { get; set; }
                /// <summary>
        /// Gets or sets the Series.
        /// </summary>
public string? Series { get; set; }
                /// <summary>
        /// Gets or sets the Main Image.
        /// </summary>
public string? MainImage { get; set; }
                /// <summary>
        /// Gets or sets the Canonical Url.
        /// </summary>
public string? CanonicalUrl { get; set; }
                /// <summary>
        /// Gets or sets the Description.
        /// </summary>
public string? Description { get; set; }
                /// <summary>
        /// Gets or sets the Tags.
        /// </summary>
public string? Tags { get; set; }
                /// <summary>
        /// Gets or sets the Organization Id.
        /// </summary>
public int? OrganizationId { get; set; }
    }

        /// <summary>
    /// Represents a class for ArticleCreateResponse.
    /// </summary>
public class ArticleCreateResponse
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
public int Id { get; set; }
                /// <summary>
        /// Gets or sets the Title.
        /// </summary>
public string? Title { get; set; }
                /// <summary>
        /// Gets or sets the Description.
        /// </summary>
public string? Description { get; set; }
                /// <summary>
        /// Gets or sets the Url.
        /// </summary>
public string? Url { get; set; }
                /// <summary>
        /// Gets or sets the Slug.
        /// </summary>
public string? Slug { get; set; }
                /// <summary>
        /// Gets or sets the Path.
        /// </summary>
public string? Path { get; set; }
                /// <summary>
        /// Gets or sets the Canonical Url.
        /// </summary>
public string? CanonicalUrl { get; set; }
                /// <summary>
        /// Gets or sets the Body Markdown.
        /// </summary>
public string? BodyMarkdown { get; set; }
                /// <summary>
        /// Gets or sets the Body Html.
        /// </summary>
public string? BodyHtml { get; set; }
    }

