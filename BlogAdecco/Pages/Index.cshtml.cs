// Copyright (c) 2025, Mapache Digital
// Version: 1.0.0
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco.Data;
using BlogAdecco.Models;
using BlogAdecco.Pages.Shared.Components.Seo;
using BlogAdecco.Utils;
using MDWidgets.Pages.Shared.Components.Pagination;
using MDWidgets.Utils;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BlogAdecco.Pages;

public class IndexModel(ApplicationDbContext _context, IBlogAdeccoUtils _blogAdeccoUtils, ISiteUtils _siteUtils) : PageModel
{
    public List<Post> Posts { get; set; } = [];
    public PaginationViewModel Pagination { get; set; } = default!;
    public int CurrentPage { get; set; }

    public async Task OnGetAsync(int? pageNumber = 1, int? pageSize = Globals.DefaultPageSize, string? search = null)
    {
        // ViewData["SeoInfo"] = new SeoInfo { ... };

        pageSize = pageSize != null && pageSize >= 3 && pageSize <= 60 ? pageSize : Globals.DefaultPageSize;
        pageNumber = pageNumber != null && pageNumber >= 1 ? pageNumber : 1;

        var posts = _context.Post
            .Include(x => x.FeaturedImage)
            .Include(x => x.Categories)
            .OrderByDescending(x => x.Fixed).ThenByDescending(x => x.Published)
            .Where(x => x.Status == PostStatus.Published);

        if (search != null)
            posts = posts.Where(x =>
                x.Title.Contains(search) ||
                x.Content.Contains(search) ||
                (x.Summary != null && x.Summary.Contains(search))
            );

        var totalPosts = posts.Count();

        posts = posts.Skip(((int)pageNumber - 1) * (int)pageSize).Take((int)pageSize);

        Posts = await posts.ToListAsync();

        var currentUrl = Request.GetDisplayUrl();
        var totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);
        var items = PaginationViewComponent.GetPagesArray((int)pageNumber, totalPages, currentUrl, (int)pageSize);

        CurrentPage = (int)pageNumber;

        Pagination = new()
        {
            Pages = items,
            PageIndex = (int)pageNumber,
            TotalPages = totalPages,
            TotalItems = totalPosts,

            FirstPageUrl = currentUrl.SetQueryString("pageNumber", 1).SetQueryString("pageSize", pageSize).SetQueryString("search", search),
            NextPageUrl = currentUrl.SetQueryString("pageNumber", pageNumber + 1 > totalPages ? totalPages : pageNumber + 1).SetQueryString("pageSize", pageSize).SetQueryString("search", search),
            PreviousPageUrl = currentUrl.SetQueryString("pageNumber", pageNumber - 1 < 1 ? 1 : pageNumber - 1).SetQueryString("pageSize", pageSize).SetQueryString("search", search),
            LastPageUrl = currentUrl.SetQueryString("pageNumber", totalPages).SetQueryString("pageSize", pageSize).SetQueryString("search", search),
        };

        var canonicalUrl = Url.PageLink("/Index");
        ViewData["SeoInfo"] = new SeoInfo
        {
            CanonicalUrl = canonicalUrl,
            Description = _siteUtils.GetSiteDescription(),
            OgInfo = new OgInfo
            {
                Url = canonicalUrl,
                Description = _siteUtils.GetSiteDescription(),
                Title = _siteUtils.GetSiteName(),
                Locale = _siteUtils.GetDefaultLanguage(),
                SiteName = _siteUtils.GetSiteName(),
            },
            TwitterInfo = new TwitterInfo
            {
                Site = "@adeccomexico",
            }
        };

        ViewData["Title"] = _siteUtils.GetSiteDescription();
    }
}
