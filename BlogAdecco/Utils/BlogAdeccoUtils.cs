// Copyright (c) 2021, Mapache Digital
// Version: 1.2
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco.Data;
using BlogAdecco.Models;
using MDWidgets.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BlogAdecco.Utils;

public interface IBlogAdeccoUtils
{     /// <summary>
      /// Obtain all the roles that are "inferior" to the current logged in user
      /// </summary>
    Task<List<string>> MySubordinatedRolesAsync();

    /// <summary>
    /// Obtain all the roles that are "inferior" to the given one
    /// </summary>
    /// <param name="myRole">The reference role</param>
    List<string> GetSubordinatedRoles(string myRole);

    /// <summary>
    /// Get the URL of a category
    /// </summary>
    Task<string?> GetCategoryUrlAsync(Category category, IUrlHelper urlHelper);

    /// <summary>
    /// Get the URL of a category
    /// </summary>
    Task<string?> GetPostUrlAsync(Post post, IUrlHelper urlHelper);

    /// <summary>
    /// Get the URL of an attachment
    /// </summary>
    /// <param name="attachment">The attachment to return when the user follows the URL</param>
    /// <param name="urlHelper">The URLHelper for the controller or page generating the link</param>
    /// <param name="thumbnail">Wheter or not return the actual attachment or just its thumbnail image</param>
    /// <returns>The URL that points to the attachment download, or its thumbnail image</returns>
    /// <exception cref="InvalidOperationException"></exception>
    Task<string?> GetAttachmentUrlAsync(Attachment attachment, IUrlHelper urlHelper, bool thumbnail = false);

    /// <summary>
    /// Create a SEO friendly slug for a category based on a string
    /// </summary>
    string GetCategorySlug(string title, int? id = null);

    /// <summary>
    /// Create a SEO friendly slug for a tag based on a string
    /// </summary>
    string GetTagSlug(string title, int? id = null);

    /// <summary>
    /// Create a SEO friendly slug for a post based on a string
    /// </summary>
    string GetPostSlug(string title, int? id = null);

    /// <summary>
    /// Create a SEO friendly slug for an attachment based on a string
    /// </summary>
    public string GetAttachmentSlug(string filename, int? id = null);
}

public partial class BlogAdeccoUtils(UserManager<ApplicationUser> _userManager, IUserUtils _userUtils, ApplicationDbContext _context) : IBlogAdeccoUtils
{
    /// <summary>
    /// Obtain all the roles that are "inferior" to the current logged in user
    /// </summary>
    public async Task<List<string>> MySubordinatedRolesAsync()
    {
        var myRole = (await _userManager.GetRolesAsync(await _userUtils.GetUserAsync())).FirstOrDefault();

        if (myRole == null) return [];

        return GetSubordinatedRoles(myRole);
    }

    /// <summary>
    /// Obtain all the roles that are "inferior" to the given one
    /// </summary>
    /// <param name="myRole">The reference role</param>
    public List<string> GetSubordinatedRoles(string myRole)
    {
        // Obtain all the roles
        var allRoles = Globals.Roles.ToList();

        // Admin is not subordinated to anyone
        allRoles.Remove(Globals.RoleAdmin);
        if (myRole == Globals.RoleAdmin) return allRoles;

        // Now the supervisor is not subordinated to anyone remaining in the roles list
        allRoles.Remove(Globals.RoleSupervisor);
        if (myRole == Globals.RoleSupervisor) return allRoles;

        // and so on...
        allRoles.Remove(Globals.RoleCompanyUser);
        return allRoles;
    }

    /// <summary>
    /// Get the URL of a category
    /// </summary>
    public async Task<string?> GetCategoryUrlAsync(Category category, IUrlHelper urlHelper)
    {
        var stack = new Stack<string>();

        stack.Push(category.Slug);

        while (category.ParentId != null)
        {
            category = await _context.Category.FirstAsync(x => x.Id == category.ParentId);
            stack.Push(category.Slug);
        }

        var values = new
        {
            Category = stack.Pop(),
            SubCategory1 = stack.Count != 0 ? stack.Pop() : null,
            SubCategory2 = stack.Count != 0 ? stack.Pop() : null,
            SubCategory3 = stack.Count != 0 ? stack.Pop() : null,
            SubCategory4 = stack.Count != 0 ? stack.Pop() : null,
            SubCategory5 = stack.Count != 0 ? stack.Pop() : null,
            SubCategory6 = stack.Count != 0 ? stack.Pop() : null,
            SubCategory7 = stack.Count != 0 ? stack.Pop() : null,
            SubCategory8 = stack.Count != 0 ? stack.Pop() : null,
            SubCategory9 = stack.Count != 0 ? stack.Pop() : null,
        };

        if (stack.Count > 0) throw new InvalidOperationException("More than 9 levels of recurssion in categories");

        var link = urlHelper.PageLink("/Categoria", values: values);

        if (!Uri.TryCreate(link, UriKind.Absolute, out _)) throw new InvalidOperationException("Generated URL is not absolute");

        return link;
    }

    /// <summary>
    /// Get the URL of a Post
    /// </summary>
    public async Task<string?> GetPostUrlAsync(Post post, IUrlHelper urlHelper)
    {
        var link = urlHelper?.PageLink("/Post", values: new
        {
            Year = post.Created.Year.ToString("0000"),
            Month = post.Created.Month.ToString("00"),
            Day = post.Created.Day.ToString("00"),
            post.Slug,
        });

        if (!Uri.TryCreate(link, UriKind.Absolute, out _)) throw new InvalidOperationException("Generated URL is not absolute");

        return link;
    }

    /// <summary>
    /// Get the URL of an attachment
    /// </summary>
    /// <param name="attachment">The attachment to return when the user follows the URL</param>
    /// <param name="urlHelper">The URLHelper for the controller or page generating the link</param>
    /// <param name="thumbnail">Wheter or not return the actual attachment or just its thumbnail image</param>
    /// <returns>The URL that points to the attachment download, or its thumbnail image</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<string?> GetAttachmentUrlAsync(Attachment attachment, IUrlHelper urlHelper, bool thumbnail = false)
    {
        var regex = GuidUrlMatch();
        var match = regex.Match(attachment.Slug);
        if (!match.Success) return null;

        string? link;

        if (!thumbnail)
        {
            link = urlHelper.PageLink("/Uploads", values: new
            {
                Year = match.Groups[1].Value,
                Month = match.Groups[2].Value,
                Path = match.Groups[3].Value,
            });
        }
        else
        {
            link = urlHelper.PageLink("/Uploads", values: new
            {
                Year = match.Groups[1].Value,
                Month = match.Groups[2].Value,
                Path = match.Groups[3].Value,
                size = "thumbnail",
            });
        }

        if (!Uri.TryCreate(link, UriKind.Absolute, out _)) throw new InvalidOperationException("Generated URL is not absolute");

        return link;
    }

    /// <summary>
    /// Regex to separate the different parts of an attachment Guid /uploads/year/month/file_path
    /// </summary>
    [GeneratedRegex(Globals.GuidRegex)]
    private static partial Regex GuidUrlMatch();

    /// <summary>
    /// Create a SEO friendly slug for a category based on a string
    /// </summary>
    public string GetCategorySlug(string name, int? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Invalid name.");
        }

        var trialSlug = name.ToSlug();

        if (!_context.Category.Any(x => x.Slug == trialSlug && x.Id != id)) return trialSlug;

        // We cannot have repeated slugs
        var retrial = 0;
        var newSlug = string.Empty;
        do
        {
            retrial++;
            newSlug = $"{trialSlug}-{retrial}";
        } while (_context.Category.Any(x => x.Slug == newSlug && x.Id != id));

        return newSlug;
    }

    /// <summary>
    /// Create a SEO friendly slug for a tag based on a string
    /// </summary>
    public string GetTagSlug(string name, int? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Invalid name.");
        }

        var trialSlug = name.ToSlug();

        if (!_context.Tag.Any(x => x.Slug == trialSlug && x.Id != id)) return trialSlug;

        // We cannot have repeated slugs
        var retrial = 0;
        var newSlug = string.Empty;
        do
        {
            retrial++;
            newSlug = $"{trialSlug}-{retrial}";
        } while (_context.Tag.Any(x => x.Slug == newSlug && x.Id != id));

        return newSlug;
    }

    /// <summary>
    /// Create a SEO friendly slug for a post based on a string
    /// </summary>
    public string GetPostSlug(string title, int? id = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Invalid name.");
        }

        var trialSlug = title.ToSlug();

        if (!_context.Post.Any(x => x.Slug == trialSlug && x.Id != id)) return trialSlug;

        // We cannot have repeated slugs
        var retrial = 0;
        var newSlug = string.Empty;
        do
        {
            retrial++;
            newSlug = $"{trialSlug}-{retrial}";
        } while (_context.Post.Any(x => x.Slug == newSlug && x.Id != id));

        return newSlug;
    }

    /// <summary>
    /// Create a SEO friendly slug for an attachment based on a string
    /// </summary>
    public string GetAttachmentSlug(string filename, int? id = null)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new InvalidOperationException("Invalid name.");
        }

        var filenamebase = Path.GetFileNameWithoutExtension(filename);
        var filenameext = Path.GetExtension(filename);

        var trialGuid = $"/uploads/{DateTime.Now.Year:D2}/{DateTime.Now.Month:D2}/{filenamebase}{filenameext}";

        if (!_context.Attachment.Any(x => x.Slug == trialGuid && x.Id != id)) return trialGuid;

        // We cannot have repeated slugs
        var retrial = 0;
        var newGuid = string.Empty;
        do
        {
            retrial++;
            newGuid = $"/uploads/{DateTime.Now.Year:D2}/{DateTime.Now.Month:D2}/{filenamebase}-{retrial}{filenameext}";
        } while (_context.Attachment.Any(x => x.Slug == newGuid && x.Id != id));

        return newGuid;
    }

    /// <summary>
    /// Return the Guid (unique URI) for an attachment edited to meet our needs, for example with the first part of the URL
    /// </summary>
    public static string FixSlug(string filename)
    {
        // Remove unwanted characters
        filename = filename.Replace(" ", "-"); // Spaces convert to hyphen
        filename = filename.Replace("–", "-"); // Dash converts to hyphen
        filename = filename.Replace("@", "-"); // @ converts to hyphen

        // Get the array of invalid characters for a filename
        char[] invalidChars = Path.GetInvalidFileNameChars();

        // Create a regex pattern to match any of the invalid characters.
        // Regex.Escape is used to ensure special regex characters in the invalidChars array
        // (like ']') are treated as literals within the character class '[...]'.
        string pattern = string.Format("[{0}]", Regex.Escape(new string(invalidChars)));

        var replacement = "-";

        // Use Regex.Replace to replace all invalid characters with the specified replacement string (default is "-").
        string sanitizedFileName = Regex.Replace(filename, pattern, replacement);


        // Optional: Ensure the result is not an empty string or just the replacement character(s)
        if (string.IsNullOrWhiteSpace(sanitizedFileName))
        {
            return "untitled"; // Or some other default name
        }

        return sanitizedFileName;
    }

    /// <summary>
    /// Checks that a file is not used in the database on more than one entity and if so, removes it
    /// </summary>
    /// <param name="_context">The database context, as this is a static function</param>
    /// <param name="type">The type of the attachment</param>
    /// <param name="attachment">The attachment to be checked</param>
    public static void RemoveAttachmentIfNotUsed(ApplicationDbContext _context, Type type, Attachment attachment)
    {
        if (type == typeof(Post))
        {
            if (_context.Attachment.Any(x => x.Posts.Count() == 0))
                _context.Attachment.Remove(attachment);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}