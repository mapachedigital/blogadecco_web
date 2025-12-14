// Copyright (c) 2025, Mapache Digital
// Version: 1.0.0
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco.Data;
using BlogAdecco.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogAdecco.Pages.Shared.Components.HeadBreadcrumb;

public class HeadBreadcrumbViewModel
{
    public string? SearchTerm { get; set; } = default!;
    public string? SearchCategory { get; set; } = default!;
    public Category? CurrentCategory { get; set; }
    public List<Category> Categories { get; set; } = [];
}

public class HeadBreadcrumbViewComponent(ApplicationDbContext _context) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _context.Category
            //.Where(x => x.Posts.Any())
            .OrderBy(x => x.Name)
            .ToListAsync();

        var searchCategory = HttpContext.Request.Query["category"];

        var model = new HeadBreadcrumbViewModel
        {
            SearchTerm = HttpContext.Request.Query["search"],
            SearchCategory = searchCategory,
            Categories = categories,
            CurrentCategory = categories.FirstOrDefault(x => x.Slug == searchCategory)
        };

        return View(model);
    }
}
