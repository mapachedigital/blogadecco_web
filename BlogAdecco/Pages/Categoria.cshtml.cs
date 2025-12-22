// Copyright (c) 2021, Mapache Digital
// Version: 1.4
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco.Data;
using BlogAdecco.Models;
using BlogAdecco.Utils;
using MDWidgets.Pages.Shared.Components.Pagination;
using MDWidgets.Pages.Shared.Components.Seo;
using MDWidgets.Utils;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BlogAdecco.Pages;

/// <summary>
/// Currently we have 9 levels of category recursion.  Must be enough for most needs.
/// </summary>
public class CategoriesModel(ApplicationDbContext _context, IBlogAdeccoUtils _blogAdeccoUtils, ISiteUtils _siteUtils) : PageModel
{
    [FromRoute]
    public string Category { get; set; } = default!;

    [FromRoute]
    public string? SubCategory1 { get; set; }

    [FromRoute]
    public string? SubCategory2 { get; set; }

    [FromRoute]
    public string? SubCategory3 { get; set; }

    [FromRoute]
    public string? SubCategory4 { get; set; }

    [FromRoute]
    public string? SubCategory5 { get; set; }

    [FromRoute]
    public string? SubCategory6 { get; set; }

    [FromRoute]
    public string? SubCategory7 { get; set; }

    [FromRoute]
    public string? SubCategory8 { get; set; }

    [FromRoute]
    public string? SubCategory9 { get; set; }
    public List<Post> Posts { get; set; } = [];
    public PaginationViewModel Pagination { get; set; } = default!;
    public int CurrentPage { get; set; }
    public Category? RequestedCategory { get; set; }

    public async Task<IActionResult> OnGetAsync(int? pageNumber = 1, int? pageSize = 15, string? search = null)
    {
        // ViewData["SeoInfo"] = new SeoInfo { ... };

        pageSize = pageSize != null && pageSize >= 3 && pageSize <= 60 ? pageSize : 15;
        pageNumber = pageNumber != null && pageNumber >= 1 ? pageNumber : 1;

        var stack = new Stack<string>();
        if (Category != null) stack.Push(Category);
        if (SubCategory1 != null) stack.Push(SubCategory1);
        if (SubCategory2 != null) stack.Push(SubCategory2);
        if (SubCategory3 != null) stack.Push(SubCategory3);
        if (SubCategory4 != null) stack.Push(SubCategory4);
        if (SubCategory5 != null) stack.Push(SubCategory5);
        if (SubCategory6 != null) stack.Push(SubCategory6);
        if (SubCategory7 != null) stack.Push(SubCategory7);
        if (SubCategory8 != null) stack.Push(SubCategory8);
        if (SubCategory9 != null) stack.Push(SubCategory9);

        //var fullCategorySlug = string.Join("/", stack.Reverse());

        // The requested category is the last one in the stack and it's the only piece of information we need to load it.
        var requestedCategorySlug = stack.Pop();

        var requestedCategory = await _context.Category
            .FirstOrDefaultAsync(x => x.Slug == requestedCategorySlug);

        if (requestedCategory == null)
            return NotFound();

        RequestedCategory = requestedCategory;

        var posts = _context.Post
            .Include(x => x.FeaturedImage)
            .Include(x => x.Categories)
            .OrderByDescending(x => x.Fixed).ThenByDescending(x => x.Created)
            .Where(x => x.Status == PostStatus.Published && x.Categories.Contains(requestedCategory));

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

        var canonicalUrl = await _blogAdeccoUtils.GetCategoryUrlAsync(requestedCategory, Url);
        ViewData["SeoInfo"] = new SeoInfo
        {
            CanonicalUrl = canonicalUrl,
            Description = requestedCategory.Description,
            OgInfo = new OgInfo
            {
                Url = canonicalUrl,
                Description = requestedCategory.Description,
                Title = requestedCategory.Name,
                Locale = _siteUtils.GetDefaultLanguage(),
                SiteName = _siteUtils.GetSiteName(),
            },
            TwitterInfo = new TwitterInfo
            {
                Site = "@adeccomexico",
            }
        };

        ViewData["Title"] = requestedCategory.Name;

        return Page();
    }
}
