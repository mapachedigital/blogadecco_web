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
    public async Task<IActionResult> Index()
    {
        return View(await _context.Attachment
            .Include(x => x.Posts)
            .OrderByDescending(x => x.Created)
            .ToListAsync());
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
