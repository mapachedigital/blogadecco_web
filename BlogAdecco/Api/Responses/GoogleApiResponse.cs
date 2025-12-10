// Copyright (c) 2025, Mapache Digital
// Version: 1
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using System.Text.Json.Serialization;

namespace BlogAdecco.Api.Responses;

/// <summary>
/// Generic response from Google API
/// </summary>
public class GoogleApiResponse
{
    [JsonPropertyName("items")]
    public List<Item>? Items { get; set; }

    /// <summary>
    /// Returns the list of videos from the response
    /// </summary>
    [JsonIgnore]
    public List<Video> Videos
    {
        get
        {
            var videos = new List<Video>();
            if (Items == null)
            {
                return videos;
            }
            foreach (var item in Items)
            {
                // Skip non-video items
                if (item.Snippet.ResourceId.Kind != "youtube#video")
                {
                    continue; 
                }

                var video = new Video
                {
                    Url = item.Snippet.ResourceId.VideoUrl,
                    Title = item.Snippet.Title,
                    ThumbnailUrl = item.Snippet.Thumbnails.High.Url,
                    Width = item.Snippet.Thumbnails.High.Width,
                    Height = item.Snippet.Thumbnails.High.Height
                };

                videos.Add(video);
            }
            return videos;
        }
    }
}


public class Item
{
    [JsonPropertyName("snippet")]
    public Snippet Snippet { get; set; } = default!;
}

public class Snippet
{
    [JsonPropertyName("resourceId")]
    public ResourceId ResourceId { get; set; } = default!;

    [JsonPropertyName("thumbnails")]
    public Thumbnails Thumbnails { get; set; } = default!;

    [JsonPropertyName("title")]
    public string Title { get; set; } = default!;
}

public class Thumbnails
{
    [JsonPropertyName("high")]
    public ThumbnailDetails High { get; set; } = default!;
}

public class ThumbnailDetails
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = default!;
    [JsonPropertyName("width")]
    public int Width { get; set; }
    [JsonPropertyName("height")]
    public int Height { get; set; }
}

public class ResourceId
{
    /// <summary>
    /// For video resources, this value is always youtube#video.
    /// </summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = default!;

    [JsonPropertyName("videoId")]
    public string VideoId { get; set; } = default!;

    public string VideoUrl => $"https://www.youtube.com/watch?v={VideoId}";
}

/// <summary>
/// ViewModel for a Youtube video
/// </summary>
public class Video
{
    /// <summary>
    /// The URL of the video
    /// </summary>
    public string Url { get; set; } = default!;

    /// <summary>
    /// The title of the video
    /// </summary>
    public string Title { get; set; } = default!;

    /// <summary>
    /// The URL of the thumbnail image
    /// </summary>
    public string ThumbnailUrl { get; set; } = default!;

    /// <summary>
    /// The width of the thumbnail image
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// The height of the thumbnail image
    /// </summary>
    public int Height { get; set; }
}
