// Copyright (c) 2021, Mapache Digital
// Version: 1.0.0
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace BlogAdecco.Pages.Shared.Components.SocialBox;

/// <summary>
/// Creates a list of social media sharing links.  If the URL or the Title is not specified then the current page data is used.
/// </summary>
public class SocialBoxViewComponent : ViewComponent
{
    /// <summary>
    /// Creates a list of social media sharing links.  If the URL or the Title is not specified then the current page data is used.
    /// </summary>
    /// <param name="title">The title shared, if the API allows it.  If empty, the current page title will be used.</param>
    /// <param name="url">The url of the page to be shared.  If empty, the current page URL will be used.</param>
    /// <returns></returns>
    public IViewComponentResult Invoke(string? title = null, string? url = null)
    {
        var request = HttpContext.Request;
        var viewData = ViewContext.ViewData;

        var currentUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

        var encodedTitle = HttpUtility.UrlEncode(title ?? viewData["Title"]?.ToString() ?? string.Empty);
        var encodedUrl = HttpUtility.UrlEncode(url ?? currentUrl ?? string.Empty);

        var model = new SocialBoxModel
        {
            FacebookShareUrl = $"https://www.facebook.com/sharer/sharer.php?u={encodedUrl}",
            TwitterShareUrl = $"https://twitter.com/intent/tweet?text={encodedTitle}&url={encodedUrl}",
            LinkedInShareUrl = $"https://www.linkedin.com/sharing/share-offsite/?url={encodedUrl}",
            WhatsappShareUrl = $"https://wa.me/?text={encodedTitle}%20{encodedUrl}"
        };

        return View(model);
    }
}
