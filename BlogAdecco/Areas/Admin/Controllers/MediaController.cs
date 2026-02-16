// Copyright (c) 2021, Mapache Digital
// Version: 1.1
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco.Areas.Admin.Models.AttachmentViewModels;
using BlogAdecco.Data;
using BlogAdecco.Models;
using BlogAdecco.Utils;
using MDWidgets;
using MDWidgets.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogAdecco.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Globals.RoleAdmin + "," + Globals.RoleSupervisor)]
public class MediaController(ApplicationDbContext _context,
        IBlogAdeccoUtils _blogAdeccoUtils,
        IUserUtils _userUtils,
        IStorageUtils _storageUtils) : Controller
{
    // GET: Admin/Attachments
    public async Task<IActionResult> Index(string? sortorder,
             string? searchString,
             int? pageNumber,
             int? pageSize,
             bool? resetCookie)
    {
        // Get a sensible page size depending on what we received from GET variable and what is stored in a cookie.
        pageSize = this.CookiePageSize(pageSize, defaultPageSize: 12);

        // Preserve the sort order between calls
        ViewData["CurrentSort"] = sortorder;

        // Default order
        sortorder = string.IsNullOrEmpty(sortorder) ? "created_desc" : sortorder;

        ViewData["CurrentFilter"] = searchString;
        ViewData["PageSize"] = pageSize == MDGlobals.PageSizeDefault ? "" : pageSize.ToString();

        // This model sorting
        ViewData["AltSortParam"] = sortorder == "alt" ? "alt_desc" : "alt";
        ViewData["SlugSortParam"] = sortorder == "slug" ? "slug_desc" : "slug";
        ViewData["TitleSortParam"] = sortorder == "title" ? "title_desc" : "title";
        ViewData["DescriptionSortParam"] = sortorder == "description" ? "description_desc" : "description";
        ViewData["OriginalFilenameSortParam"] = sortorder == "original_filename" ? "original_filename_desc" : "original_filename";
        ViewData["CreatedSortParamSortParam"] = sortorder == "created" ? "created_desc" : "created";

        // Obtain the items from the database        
        var applicationDbContext = _context.Attachment.Select(x => x);

        // Perform the search
        searchString = searchString?.Trim();
        if (!string.IsNullOrEmpty(searchString))
        {
            applicationDbContext = applicationDbContext.Where(s =>
                s.Slug.Contains(searchString) ||
                (s.Alt != null && s.Alt.Contains(searchString)) ||
                (s.Title != null && s.Title.Contains(searchString)) ||
                (s.Description != null && s.Description.Contains(searchString)) ||
                (s.OriginalFilename != null && s.OriginalFilename.Contains(searchString)));
        }

        // Reset the pagination when a new search is performed
        if (resetCookie == true)
        {
            pageNumber = 1;
        }

        applicationDbContext = sortorder switch
        {
            "alt" => applicationDbContext.OrderBy(s => s.Alt),
            "alt_desc" => applicationDbContext.OrderByDescending(s => s.Alt),
            "slug" => applicationDbContext.OrderBy(s => s.Slug),
            "slug_desc" => applicationDbContext.OrderByDescending(s => s.Slug),
            "title" => applicationDbContext.OrderBy(s => s.Title),
            "title_desc" => applicationDbContext.OrderByDescending(s => s.Title),
            "description" => applicationDbContext.OrderBy(s => s.Description),
            "description_desc" => applicationDbContext.OrderByDescending(s => s.Description),
            "original_filename" => applicationDbContext.OrderBy(s => s.OriginalFilename),
            "original_filename_desc" => applicationDbContext.OrderByDescending(s => s.OriginalFilename),
            "created" => applicationDbContext.OrderBy(s => s.Created),
            "created_desc" => applicationDbContext.OrderByDescending(s => s.Created),
            "created_by" => applicationDbContext.OrderBy(s => s.CreatedBy != null ? s.CreatedBy.DisplayName : ""),
            "created_by_desc" => applicationDbContext.OrderByDescending(s => s.CreatedBy != null ? s.CreatedBy.DisplayName : ""),
            _ => applicationDbContext.OrderByDescending(s => s.Created),
        };

        return View(await PaginatedList<Attachment>.CreateAsync(applicationDbContext.AsNoTracking(), pageNumber ?? 1, pageSize ?? MDGlobals.PageSizeDefault));
    }

    // GET: Admin/Attachments/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var attachment = await _context.Attachment
            .Include(c => c.CreatedBy)
            .Include(p => p.Posts)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (attachment == null)
        {
            return NotFound();
        }

        return View(attachment);
    }

    // GET: Admin/Attachments/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Admin/Attachments/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Title,Alt,Description,File")] AttachmentCreateViewModel attachmentVM)
    {
        var user = await _userUtils.GetUserAsync();

        if (ModelState.IsValid)
        {
            using var fileStream = attachmentVM.File.OpenReadStream();
            var filename = attachmentVM.File.FileName.Trim();
            var contentType = attachmentVM.File.ContentType.Trim();
            var storedFile = await _storageUtils.UploadFileAsync(fileStream, contentType, Globals.StorageContainerNameAttachments);

            var attachment = new Attachment
            {
                Title = attachmentVM.Title?.Trim(),
                Alt = attachmentVM.Alt?.Trim(),
                Description = attachmentVM.Description?.Trim(),

                File = storedFile.Name,
                Container = storedFile.Container,
                Location = storedFile.Location,
                OriginalFilename = filename,
                MimeType = contentType,
                Created = DateTime.Now,
                CreatedById = user.Id,

                Slug = _blogAdeccoUtils.GetAttachmentSlug(BlogAdeccoUtils.FixSlug(filename)),
            };

            // If the attachment is an image, create a thumbnail
            if (MDGlobals.AcceptedImages.Contains(attachmentVM.File.ContentType))
            {
                fileStream.Position = 0;
                fileStream.Seek(0, SeekOrigin.Begin);

                // Create the thumbnail
                var image = ImageUtils.CreateThumbnail(fileStream, contentType, MDGlobals.ThumbWidth);
                if (image != null)
                {
                    var thumbnail = await _storageUtils.UploadFileAsync(image, contentType, attachment.Container);
                    attachment.ThumbFile = thumbnail.Name;
                    attachment.ThumbContainer = thumbnail.Container;
                }
            }

            _context.Add(attachment);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(attachmentVM);
    }

    // GET: Admin/Attachments/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var attachment = await _context.Attachment.FindAsync(id);
        if (attachment == null)
        {
            return NotFound();
        }

        var attachmentVM = new AttachmentEditViewModel
        {
            Id = attachment.Id,
            Title = attachment.Title,
            Alt = attachment.Alt,
            Description = attachment.Description,
        };

        ViewData["AttachmentUrl"] = await _blogAdeccoUtils.GetAttachmentUrlAsync(attachment, Url);

        return View(attachmentVM);
    }

    // POST: Admin/Attachments/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Alt,Description")] AttachmentEditViewModel attachmentVM)
    {
        if (id != attachmentVM.Id)
        {
            return NotFound();
        }

        var attachment = await _context.Attachment.FindAsync(id);
        if (attachment == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            attachment.Title = attachmentVM.Title?.Trim();
            attachment.Alt = attachmentVM.Alt?.Trim();
            attachment.Description = attachmentVM.Description?.Trim();

            try
            {
                _context.Update(attachment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AttachmentExists(attachment.Id))
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

        ViewData["AttachmentUrl"] = await _blogAdeccoUtils.GetAttachmentUrlAsync(attachment, Url);

        return View();
    }

    // GET: Admin/Attachments/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var attachment = await _context.Attachment
            .Include(c => c.CreatedBy)
            .Include(p => p.Posts)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (attachment == null)
        {
            return NotFound();
        }

        return View(attachment);
    }

    // POST: Admin/Attachments/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var attachment = await _context.Attachment
            .Include(c => c.CreatedBy)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (attachment == null)
        {
            return NotFound();
        }

        foreach (var item in _context.Post.Where(x => x.FeaturedImageId == id))
        {
            item.FeaturedImageId = null;
            _context.Update(item);
        }

        await _storageUtils.RemoveFileAsync(attachment.File, attachment.Container, attachment.Location);
        if (attachment.ThumbFile != null && attachment.ThumbContainer != null)
        {
            await _storageUtils.RemoveFileAsync(attachment.ThumbFile, attachment.ThumbContainer, attachment.Location);
        }

        _context.Attachment.Remove(attachment);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool AttachmentExists(int id)
    {
        return _context.Attachment.Any(e => e.Id == id);
    }
}
