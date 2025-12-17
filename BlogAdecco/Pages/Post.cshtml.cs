// Copyright (c) 2021, Mapache Digital
// Version: 1.4
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco.Data;
using BlogAdecco.Models;
using BlogAdecco.Pages.Shared.Components.Seo;
using BlogAdecco.Utils;
using MDWidgets.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BlogAdecco.Pages;

public class PostModel(ApplicationDbContext _context, IBlogAdeccoUtils _blogAdeccoUtils, ISiteUtils _siteUtils) : PageModel
{
    [FromRoute]
    public int Year { get; set; }

    [FromRoute]
    public int Month { get; set; }

    [FromRoute]
    public int Day { get; set; }

    [FromRoute]
    public string Slug { get; set; } = default!;

    public Post Post { get; set; } = default!;

    public List<Category> RelatedCategories { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var post = await _context.Post
            .Include(x => x.FeaturedImage)
            .Include(x => x.Categories)
            .Include(x => x.Tags)
            .Where(x => x.Status == PostStatus.Published)
            .FirstOrDefaultAsync(x => x.Slug == Slug);

        if (post == null) return NotFound();

        var postMainCategoryId = post.Categories.FirstOrDefault()?.Id;
        var relatedCategories = await _context.Category
            .Where(c => c.Id != postMainCategoryId && c.Posts.Any())
            .OrderByDescending(c => c.Posts.Count())
            .ThenBy(c => c.Name)
            .Take(5)
            .ToListAsync();

        Post = post;
        RelatedCategories = relatedCategories;

        var canonicalUrl = await _blogAdeccoUtils.GetPostUrlAsync(post, Url);
        var featuredImageUrl = post.FeaturedImageId != null ? Url.ActionLink("File", "Home", new { area = "Attachments", Id = post.FeaturedImageId }) : null;
        ViewData["SeoInfo"] = new SeoInfo
        {
            CanonicalUrl = canonicalUrl,
            Description = post.Summary,
            OgInfo = new OgInfo
            {
                Url = canonicalUrl,
                Description = post.Summary,
                Title = post.Title,
                Locale = _siteUtils.GetDefaultLanguage(),
                SiteName = _siteUtils.GetSiteName(),
                Article = new()
                {
                    PublishedTime = post.Published,
                    ModifiedTime = post.Modified,
                    Section = post.Categories?.FirstOrDefault()?.Name,
                    Tags = [.. post.Tags.Select(x => x.Name)],
                },
                Type = "article",
                Image = featuredImageUrl,
            },
            TwitterInfo = new TwitterInfo
            {
                Site = "@adeccomexico",
            }
        };
        return Page();
    }
}
