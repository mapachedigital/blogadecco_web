using BlogAdecco.Areas.Admin.Models.PostViewModels;
using BlogAdecco.Data;
using BlogAdecco.Models;
using BlogAdecco.Utils;
using MDWidgets;
using MDWidgets.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BlogAdecco.Areas.Admin.Controllers;

// Copyright (c) 2021, Mapache Digital
// Version: 1.1
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com
[Area("Admin")]
[Authorize(Roles = Globals.RoleAdmin + "," + Globals.RoleSupervisor)]
public class PostsController(ApplicationDbContext _context, IUserUtils _userUtils, IStorageUtils _storageUtils, IBlogAdeccoUtils _blogAdeccoUtils) : Controller
{
    // GET: Admin/Posts
    public async Task<IActionResult> Index()
    {
        return View(await _context.Post
            .OrderByDescending(x => x.Created)
            .Include(x => x.CreatedBy)
            .Include(x => x.Categories)
            .Include(x => x.Tags)
            .ToListAsync());
    }

    // GET: Admin/Posts/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Post
            .Include(x => x.FeaturedImage)
            .Include(x => x.CreatedBy)
            .Include(x => x.ModifiedBy)
            .Include(x => x.Categories)
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        return View(post);
    }

    // GET: Admin/Posts/Create
    public IActionResult Create()
    {
        ViewData["Categories"] = new SelectList(_context.Category.OrderBy(x => x.Name), "Id", "Name");
        ViewData["Tags"] = new SelectList(_context.Tag.OrderBy(x => x.Name), "Id", "Name");
        return View();
    }

    // POST: Admin/Posts/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Title,Summary,Content,FeaturedImage,Status,CategoryId,TagId,Fixed")] PostCreateViewModel postVM)
    {
        var user = await _userUtils.GetUserAsync();

        if (ModelState.IsValid)
        {
            int? attachmentId = null;

            // Process the post thumbnail
            if (postVM.FeaturedImage != null)
            {
                using var fileStream = postVM.FeaturedImage.OpenReadStream();
                var filename = postVM.FeaturedImage.FileName.Trim();
                var contentType = postVM.FeaturedImage.ContentType.Trim();
                var storedFile = await _storageUtils.UploadFileAsync(fileStream, contentType, Globals.StorageContainerNameAttachments);

                var attachment = new Attachment
                {
                    //Title = attachmentVM.Title?.Trim(),
                    //Alt = attachmentVM.Alt?.Trim(),
                    //Description = attachmentVM.Description?.Trim(),

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
                if (MDGlobals.AcceptedImages.Contains(postVM.FeaturedImage.ContentType))
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

                _context.Attachment.Add(attachment);
                await _context.SaveChangesAsync();

                attachmentId = attachment.Id;
                fileStream.Close();
            }

            var post = new Post
            {
                Title = postVM.Title.Trim(),
                Summary = postVM.Summary?.Trim(),
                Content = postVM.Content.Trim(),
                FeaturedImageId = attachmentId,
                Status = postVM.Status,
                Slug = _blogAdeccoUtils.GetPostSlug(postVM.Title),
                Created = DateTime.Now,
                CreatedById = user.Id,
                Categories = [.. _context.Category.Where(x => postVM.CategoryId.Contains(x.Id))],
                Tags = [.. _context.Tag.Where(x => postVM.TagId.Contains(x.Id))],
                Fixed = postVM.Fixed,
                Published = postVM.Status == PostStatus.Published ? DateTime.Now : null,
            };

            _context.Add(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["Categories"] = new SelectList(_context.Category.OrderBy(x => x.Name), "Id", "Name");
        ViewData["Tags"] = new SelectList(_context.Tag.OrderBy(x => x.Name), "Id", "Name");
        return View(postVM);
    }

    // GET: Admin/Posts/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Post
            .Include(x => x.FeaturedImage)
            .Include(x => x.Categories)
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        var PostVM = new PostCreateViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Status = post.Status,
            Summary = post.Summary,
            Fixed = post.Fixed,
            CategoryId = [.. post.Categories.Select(x => x.Id)],
            TagId = [.. post.Tags.Select(x => x.Id)]
        };

        ViewData["FeaturedImage"] = post.FeaturedImage;
        ViewData["Categories"] = new SelectList(_context.Category.OrderBy(x => x.Name), "Id", "Name", post.Categories.Select(x => x.Id));
        ViewData["Tags"] = new SelectList(_context.Tag.OrderBy(x => x.Name), "Id", "Name", post.Tags.Select(x => x.Id));

        return View(PostVM);
    }

    // POST: Admin/Posts/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Summary,Content,FeaturedImage,Status,CategoryId,TagId,Fixed")] PostCreateViewModel postVM)
    {
        if (id != postVM.Id)
        {
            return NotFound();
        }

        var user = await _userUtils.GetUserAsync();

        var post = await _context.Post
            .Include(x => x.FeaturedImage)
            .Include(x => x.Categories)
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.Id == postVM.Id);

        if (post == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                int? attachmentId = null;
                if (postVM.FeaturedImage != null)
                {
                    using var fileStream = postVM.FeaturedImage.OpenReadStream();
                    var filename = postVM.FeaturedImage.FileName.Trim();
                    var contentType = postVM.FeaturedImage.ContentType.Trim();
                    var storedFile = await _storageUtils.UploadFileAsync(fileStream, contentType, Globals.StorageContainerNameAttachments);

                    var attachment = new Attachment
                    {
                        //Title = attachmentVM.Title?.Trim(),
                        //Alt = attachmentVM.Alt?.Trim(),
                        //Description = attachmentVM.Description?.Trim(),

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
                    if (MDGlobals.AcceptedImages.Contains(postVM.FeaturedImage.ContentType))
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

                    _context.Attachment.Add(attachment);
                    await _context.SaveChangesAsync();

                    attachmentId = attachment.Id;
                    fileStream.Close();
                }

                post.Title = postVM.Title.Trim();
                post.Summary = postVM.Summary?.Trim();
                post.Content = postVM.Content.Trim();
                post.Status = postVM.Status;
                post.Fixed = postVM.Fixed;
                post.Categories = [.. _context.Category.Where(x => postVM.CategoryId.Contains(x.Id))];
                post.Tags = [.. _context.Tag.Where(x => postVM.TagId.Contains(x.Id))];
                post.Modified = DateTime.Now;
                post.ModifiedById = user.Id;
                post.Published = postVM.Status != PostStatus.Published ? null : (post.Published == null ? DateTime.Now : post.Published);

                if (attachmentId != null)
                {
                    if (post.FeaturedImage != null)
                    {
                        await _storageUtils.RemoveFileAsync(post.FeaturedImage.File, post.FeaturedImage.Container, post.FeaturedImage.Location);

                        if (post.FeaturedImage.ThumbFile != null)
                            await _storageUtils.RemoveFileAsync(post.FeaturedImage.ThumbFile, post.FeaturedImage.Container, post.FeaturedImage.Location);

                        BlogAdeccoUtils.RemoveAttachmentIfNotUsed(_context, typeof(Post), post.FeaturedImage);
                    }
                    post.FeaturedImageId = attachmentId;
                }

                _context.Update(post);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(postVM.Id))
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

        ViewData["FeaturedImage"] = post.FeaturedImage;
        ViewData["Categories"] = new SelectList(_context.Category.OrderBy(x => x.Name), "Id", "Name", post.Categories.Select(x => x.Id));
        ViewData["Tags"] = new SelectList(_context.Tag.OrderBy(x => x.Name), "Id", "Name", post.Tags.Select(x => x.Id));

        return View(postVM);
    }

    // GET: Admin/Posts/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Post
            .Include(x => x.FeaturedImage)
            .Include(x => x.CreatedBy)
            .Include(x => x.ModifiedBy)
            .Include(x => x.Categories)
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (post == null)
        {
            return NotFound();
        }

        return View(post);
    }

    // POST: Admin/Posts/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var post = await _context.Post
            .Include(x => x.FeaturedImage)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (post != null)
        {
            _context.Post.Remove(post);

            if (post.FeaturedImage != null)
            {
                await _storageUtils.RemoveFileAsync(post.FeaturedImage.File, post.FeaturedImage.Container, post.FeaturedImage.Location);

                if (post.FeaturedImage.ThumbFile != null)
                    await _storageUtils.RemoveFileAsync(post.FeaturedImage.ThumbFile, post.FeaturedImage.Container, post.FeaturedImage.Location);

                BlogAdeccoUtils.RemoveAttachmentIfNotUsed(_context, typeof(Post), post.FeaturedImage);
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool PostExists(int id)
    {
        return _context.Post.Any(e => e.Id == id);
    }
}
