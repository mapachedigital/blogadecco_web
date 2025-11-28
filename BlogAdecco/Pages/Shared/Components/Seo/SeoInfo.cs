// Copyright (c) 2021, Mapache Digital
// Version: 1.0.0
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

namespace BlogAdecco.Pages.Shared.Components.Seo;

public class SeoInfo
{
    /// <summary>
    /// Use this tag to provide a short description of the page. In some situations, this description is used in the snippet shown in search results.
    /// </summary>
    public string? Description { get; set; }

    /// <summary> 
    /// Gets or sets the canonical URL associated with the content.
    /// </summary>
    /// <remarks>Use this property to specify the preferred URL for search engines and external references.
    /// Setting a canonical URL helps avoid duplicate content issues and improves SEO by indicating the authoritative
    /// address for the resource.</remarks>
    public string? CanonicalUrl { get; set; }

    /// <summary>
    /// These meta tags control the behavior of search engine crawling and indexing. 
    /// <see href="https://developers.google.com/search/docs/crawling-indexing/robots-meta-tag#directives"/>
    /// </summary>
    public string? Robots { get; set; }

    /// <summary>
    /// Open Graph information
    /// </summary>
    public OgInfo? OgInfo { get; set; }

    /// <summary>
    /// X.com Open Graph information
    /// </summary>
    public TwitterInfo? TwitterInfo { get; set; }

    /// <summary>
    /// You can use this tag on the top-level page of your site to verify ownership for Search Console.
    /// </summary>
    public string? GoogleSiteVerification { get; set; }
}

public class OgInfo
{
    /// <summary>
    /// The title of your object as it should appear within the graph, e.g., "The Rock".
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The type of your object, e.g., "video.movie". Depending on the type you specify, other properties may also be required.
    /// <see href="https://ogp.me/#types"/>
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// An image URL which should represent your object within the graph.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// The canonical URL of your object that will be used as its permanent ID in the graph, e.g., "https://www.imdb.com/title/tt0117500/".
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// A one to two sentence description of your object.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The locale these tags are marked up in. Of the format language_TERRITORY. Default is en_US.
    /// </summary>
    public string? Locale { get; set; }

    /// <summary>
    /// If your object is part of a larger web site, the name which should be displayed for the overall site. e.g., "IMDb".
    /// </summary>
    public string? SiteName { get; set; }

    /// <summary>
    /// A URL to a video file that complements this object.
    /// </summary>
    public string? Video { get; set; }
}

public class TwitterInfo
{
    /// <summary>
    /// The card type, which will be one of “summary”, “summary_large_image”, “app”, or “player”.
    /// </summary>
    public string? Card { get; set; }

    /// <summary>
    /// @username for the website used in the card footer.
    /// </summary>
    public string? Site { get; set; }

    /// <summary>
    /// @username for the content creator / author.
    /// </summary>
    public string? Creator { get; set; }
}