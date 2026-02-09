// Copyright (c) 2021, Mapache Digital
// Version: 1.1
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco.Areas.Admin.Models.TagViewModels;
using BlogAdecco.Data;
using BlogAdecco.Models;
using BlogAdecco.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BlogAdecco.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Globals.RoleAdmin + "," + Globals.RoleSupervisor)]
public class TagsController(ApplicationDbContext _context,
        IBlogAdeccoUtils _blogAdeccoUtils) : Controller
{
    // GET: Admin/Tags
    public async Task<IActionResult> Index()
    {
        return View(await _context.Tag
            .Include(x => x.Children)
            .Include(x => x.Parent)
            .Include(x => x.Posts)
            .OrderBy(x => x.Parent!.Name).ThenBy(x => x.Name)
            .ToListAsync());
    }

    // GET: Admin/Tags/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tag = await _context.Tag
            .Include(x => x.Posts)
            .Include(x => x.Parent)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (tag == null)
        {
            return NotFound();
        }

        return View(tag);
    }

    // GET: Admin/Tags/Create
    public IActionResult Create()
    {
        ViewData["ParentId"] = new SelectList(_context.Tag.OrderBy(x => x.Name), "Id", "Name");
        return View();
    }

    // POST: Admin/Tags/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Description,ParentId")] TagCreateViewModel tagVM)
    {
        if (ModelState.IsValid)
        {
            var tag = new Tag
            {
                Name = tagVM.Name.Trim(),
                ParentId = tagVM.ParentId,
                Slug = _blogAdeccoUtils.GetTagSlug(tagVM.Name),
            };

            _context.Add(tag);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["ParentId"] = new SelectList(_context.Tag.OrderBy(x => x.Name), "Id", "Name", tagVM.ParentId);

        return View(tagVM);
    }

    // GET: Admin/Tags/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tag = await _context.Tag.FindAsync(id);
        if (tag == null)
        {
            return NotFound();
        }

        var tagVM = new TagCreateViewModel
        {
            Id = tag.Id,
            Name = tag.Name,
            ParentId = tag.ParentId,
        };

        ViewData["ParentId"] = new SelectList(_context.Tag.OrderBy(x => x.Name), "Id", "Name", tagVM.ParentId);

        return View(tagVM);
    }

    // POST: Admin/Tags/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ParentId")] TagCreateViewModel tagVM)
    {
        if (id != tagVM.Id)
        {
            return NotFound();
        }

        var tag = await _context.Tag.FindAsync(id);
        if (tag == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            tag.Name = tagVM.Name.Trim();
            tag.ParentId = tagVM.ParentId;

            try
            {
                _context.Update(tag);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagExists(tag.Id))
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

        ViewData["ParentId"] = new SelectList(_context.Tag.OrderBy(x => x.Name), "Id", "Name", tagVM.ParentId);

        return View();
    }

    // GET: Admin/Tags/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tag = await _context.Tag
            .Include(x => x.Posts)
            .Include(x => x.Children)
            .Include(x => x.Parent)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (tag == null)
        {
            return NotFound();
        }

        if (tag.Children.Count > 0)
        {
            throw new InvalidOperationException("Can't delete a tag with children");
        }

        return View(tag);
    }

    // POST: Admin/Tags/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var tag = await _context.Tag
            .Include(x => x.Posts).ThenInclude(x => x.Tags)
            .Include(x => x.Children)
            .Include(x => x.Parent)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (tag == null)
        {
            return NotFound();
        }

        foreach (var item in tag.Posts)
        {
            item.Tags.Remove(tag);
            _context.Update(item);
        }

        _context.Tag.Remove(tag);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TagExists(int id)
    {
        return _context.Tag.Any(e => e.Id == id);
    }
}
