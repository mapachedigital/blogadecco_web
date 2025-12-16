// Copyright (c) 2021, Mapache Digital
// Version: 1.4
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco.Data;
using BlogAdecco.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BlogAdecco.Pages;

public class PostModel(ApplicationDbContext _context) : PageModel
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

    public async Task<IActionResult> OnGet()
    {
        var post = await _context.Post
            .Include(x => x.FeaturedImage)
            .Include(x => x.Categories)
            .Include(x => x.Tags)
            .Where(x => x.Status == PostStatus.Published)
            .FirstOrDefaultAsync(x => x.Slug == Slug && x.Created == new DateTime(Year, Month, Day));

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

        return Page();
    }
}
