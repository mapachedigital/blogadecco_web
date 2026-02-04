// Copyright (c) 2021, Mapache Digital
// Version: 1.1
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco.Areas.Admin.Models.CategoryViewModels;
using BlogAdecco.Data;
using BlogAdecco.Models;
using BlogAdecco.Utils;
using MDWidgets.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace BlogAdecco.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Globals.RoleAdmin + "," + Globals.RoleSupervisor)]
public class CategoriesController(ApplicationDbContext _context,
        IStringLocalizer<SharedResources> L,
        IBlogAdeccoUtils _blogAdeccoUtils) : Controller
{

    // GET: Admin/Categories
    public async Task<IActionResult> Index()
    {
        return View(await _context.Category
            .Include(x => x.Children)
            .Include(x => x.Parent)
            .Include(x => x.Posts)
            .OrderBy(x => x.Parent!.Name).ThenBy(x => x.Name)
            .ToListAsync());
    }

    // GET: Admin/Categories/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Category
            .Include(x => x.Posts)
            .Include(x => x.Parent)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // GET: Admin/Categories/Create
    public IActionResult Create()
    {
        ViewData["ParentId"] = new SelectList(_context.Category.OrderBy(x => x.Name), "Id", "Name");
        return View();
    }

    // POST: Admin/Categories/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Description,ParentId")] CategoryCreateViewModel categoryVM)
    {
        if (ModelState.IsValid)
        {
            var category = new Category
            {
                Name = categoryVM.Name.Trim(),
                Description = categoryVM.Description?.Trim(),
                ParentId = categoryVM.ParentId,
                Slug = _blogAdeccoUtils.GetCategorySlug(categoryVM.Name),
            };

            _context.Add(category);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["ParentId"] = new SelectList(_context.Category.OrderBy(x => x.Name), "Id", "Name", categoryVM.ParentId);

        return View(categoryVM);
    }

    // GET: Admin/Categories/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Category.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        var categoryVM = new CategoryCreateViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentId = category.ParentId,
        };

        ViewData["ParentId"] = new SelectList(_context.Category.OrderBy(x => x.Name), "Id", "Name", categoryVM.ParentId);

        return View(categoryVM);
    }

    // POST: Admin/Categories/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ParentId")] CategoryCreateViewModel categoryVM)
    {
        if (id != categoryVM.Id)
        {
            return NotFound();
        }

        var category = await _context.Category.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            category.Name = categoryVM.Name.Trim();
            category.ParentId = categoryVM.ParentId;
            category.Description = categoryVM.Description?.Trim();

            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["ParentId"] = new SelectList(_context.Category.OrderBy(x => x.Name), "Id", "Name", categoryVM.ParentId);

        return View();
    }

    // GET: Admin/Categories/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Category
            .Include(x => x.Posts)
            .Include(x => x.Children)
            .Include(x => x.Parent)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        if (category.Children.Count > 0)
        {
            throw new InvalidOperationException("Can't delete a category with children");
        }

        var categoryTypeVM = new CategoryDeleteViewModel
        {
            Id = category.Id,
            NewId = null,
            Posts = category.Posts,
            Children = category.Children,
            Description = category.Description,
            ParentId = category.ParentId,
            Slug = category.Slug,
            Parent = category.Parent,
            Name = category.Name,
        };

        var newCategory = _context.Category.Where(x => x.Id != category.Id);

        ViewData["NewId"] = new SelectList(newCategory, "Id", "Name");

        return View(categoryTypeVM);
    }

    // POST: Admin/Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, [Bind("Id,NewId")] CategoryDeleteViewModel categoryVM)
    {
        var category = await _context.Category
            .Include(x => x.Posts).ThenInclude(x => x.Categories)
            .Include(x => x.Children)
            .Include(x => x.Parent)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (category == null)
        {
            return NotFound();
        }

        var isUsed = category.Posts.Count != 0;

        if (isUsed && categoryVM.NewId == null || categoryVM.NewId == id)
        {
            ModelState.AddModelError("NewId", L["Please select a valid option."]);

            var newId = _context.Category.Where(x => x.Id != category.Id);
            categoryVM.Posts = category.Posts;
            categoryVM.Children = category.Children;
            categoryVM.Description = category.Description;
            categoryVM.ParentId = category.ParentId;
            categoryVM.Slug = category.Slug;
            categoryVM.Parent = category.Parent;
            categoryVM.Name = category.Name;

            ViewData["NewId"] = new SelectList(newId, "Id", "Name", categoryVM.NewId);

            return View(nameof(Delete), categoryVM);
        }

        var newCategory = await _context.Category.FirstOrDefaultAsync(x => x.Id == categoryVM.NewId);

        foreach (var item in category.Posts)
        {
            if (categoryVM.NewId != null)
            {
                item.Categories.Remove(category);
                if (newCategory != null && !item.Categories.Contains(newCategory))
                {
                    item.Categories.Add(newCategory);
                }
            }
            _context.Update(item);
        }

        _context.Category.Remove(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CategoryExists(int id)
    {
        return _context.Category.Any(e => e.Id == id);
    }
}
