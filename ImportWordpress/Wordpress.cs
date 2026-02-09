// Copyright (c) 2021, Mapache Digital
// Version: 1.2.4
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco;
using BlogAdecco.Data;
using BlogAdecco.Models;
using BlogAdecco.Utils;
using Ganss.Xss;
using ImportWordpress.Data;
using MDWidgets;
using MDWidgets.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhpSerializerNET;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ImportWordpress;

/// <summary>
/// Imports a wordpress database into our own blog database structure
/// </summary>
public partial class ImportWordpress
{
    /// <summary>
    /// This holds the UserID to which all the imported records will belong (so far we don't import wordpress users).
    /// </summary>
    private string adminId = default!;

    /// <summary>
    /// An HTML sanitizer to avoid code injection
    /// </summary>
    private readonly HtmlSanitizer sanitizer = new();

    /// <summary>
    /// This is the location of the uploads folder of the wordpress
    /// </summary>
    private readonly string oldUploadsPath = default!;

    /// <summary>
    /// This is the location of the local destination for the copied uploads
    /// </summary>
    private readonly string destinationUploadsPath = default!;

    /// <summary>
    /// The database context for our blow that we're importing into
    /// </summary>
    private readonly ApplicationDbContext _blogContext;

    /// <summary>
    /// The database context for the existing wordpress (MySql)
    /// </summary>
    private readonly WordpressContext _wpContext;

    /// <summary>
    /// The configuration manager for this app
    /// </summary>
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Injected IUserUtils library
    /// </summary>
    private readonly IUserUtils _userUtils;

    /// <summary>
    /// The sites base urls we're importing
    /// </summary>
    private readonly List<string> _sitesToImport = [];

    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStorageUtils _storageUtils;
    private readonly Dictionary<ulong, string> userList = [];

    /// <summary>
    /// Class constructor
    /// </summary>
    public ImportWordpress(ApplicationDbContext blogContext,
        WordpressContext wpContext,
        IConfiguration configuration,
        IUserUtils userUtils,
        UserManager<ApplicationUser> userManager,
        IConfigUtils configUtils,
        IUserStore<ApplicationUser> userStore,
        IStorageUtils storageUtils)
    {
        _blogContext = blogContext;
        _wpContext = wpContext;
        _configuration = configuration;
        _userUtils = userUtils;
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = (IUserEmailStore<ApplicationUser>)_userStore;
        _storageUtils = storageUtils;

        // Check the configuration
        configUtils.CheckConfig();

        // Define the HTML sanitizer allowances
        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedCssProperties.Clear();
        sanitizer.AllowedAtRules.Clear();
        sanitizer.AllowedSchemes.Clear();
        sanitizer.RemovingTag += Sanitizer_RemovingTag;

        // Define the filesystem paths for attachments import
        oldUploadsPath = _configuration["WordpressImport:OldUploadsPath"] ?? throw new InvalidOperationException("WordpressImport:OldUploadsPath not defined in the appsettings or secrets.");
        destinationUploadsPath = _configuration["WordpressImport:DestinationUploadsPath"] ?? throw new InvalidOperationException("WordpressImport: DestinationUploadsPath not defined in the appsettings or secrets.");

        // Remove data from existing previous (suerly failed) migrations
        if (Directory.Exists(destinationUploadsPath)) Directory.Delete(destinationUploadsPath, true);

        // Get the base URL of the site to import
        _sitesToImport = configuration.GetSection("WordpressImport:Sites").Get<List<string>>() ?? throw new InvalidOperationException("WordpressImport:Sites not defined as an array of strings");
    }

    /// <summary>
    /// Perform the import
    /// </summary>
    public async Task ImportAsync()
    {
        // Initialize the database
        await InitializeDatabaseAsync();

        // Fix inconsistent data
        var rows = await FixDataAsync();
        Console.WriteLine($"Fixed {rows} rows with incorrect dates");

        // Import the different entities
        await ImportTagsAsync();
        await ImportCategoriesAsync();
        await ImportAttachmentsAsync();
        await ImportPostsAsync();
    }

    /// <summary>
    /// Initialize the dfestination database
    /// </summary>
    private async Task InitializeDatabaseAsync()
    {
        // Remove the database
        await _blogContext.Database.EnsureDeletedAsync();

        // Create it again
        await _blogContext.Database.MigrateAsync();

        // Create the super admin
        await _userUtils.InitializeRolesAsync();

        // Get the admin user of the database which will own the entities imported (no user import yet implemented).
        var user = _blogContext.ApplicationUser.First();
        adminId = user?.Id ?? throw new InvalidOperationException("Need at least one user");
    }

    /// <summary>
    /// Validates the models according to the rules defined in the .NET models
    /// </summary>
    /// <param name="model">The model instance to be validated</param>
    /// <exception cref="InvalidOperationException">Thrown in case the model fails to validate</exception>
    private static void TryValidate(object model)
    {
        List<ValidationResult> validationResults = [];
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);
        if (!Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true))
        {
            // Print a list of properties not validating on the model
            foreach (var result in validationResults)
            {
                Console.WriteLine($"Validation Error for {result.MemberNames.FirstOrDefault()}: {result.ErrorMessage}");
            }
            throw new InvalidOperationException($"Validation Error {model.GetType()}");
        }
    }

    /// <summary>
    /// Preserve the content of the removed HTML elements.
    /// <example>For example <![CDATA[<a href="link">Text</a>]]> will become Text.</example>
    /// </summary>
    private void Sanitizer_RemovingTag(object? sender, RemovingTagEventArgs e)
    {
        e.Tag.OuterHtml = sanitizer.Sanitize(e.Tag.InnerHtml);
        e.Cancel = true;
    }

    /// <summary>
    /// Create a text only summary of the given HTML text.
    /// </summary>
    private string CreateSummary(string content)
    {
        // Remove HTML tags
        var result = sanitizer.Sanitize(content);

        // Change &nbpsp; to regular spaces
        result = NBSpace().Replace(result, " ");

        // Convert different space characters to the " "
        result = OneSpacheChar().Replace(result, " ");

        // Collapse sequence of spaces to just one
        result = ManySpaceChars().Replace(result, " ");

        // Trim the string to the first words
        return result.TrimWords(30);
    }

    /// <summary>
    /// Remove non standard text characters from a string
    /// </summary>
    private static string RemoveStrangeCharacters(string input)
    {
        return input
            // "Unspecified object", when the user copies/paste from one app to another that doesn't support what is being copied
            .Replace("\ufffc", "")
            .Replace("%EF%BF%BC", "", StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Fix inconsistent data in the database.
    /// </summary>
    /// <returns></returns>
    private async Task<int> FixDataAsync()
    {
        // MySQL allows for an all zeros date, while SQLServer doesn't.
        var rowsAffected1 = await _wpContext.Database.ExecuteSqlAsync($"UPDATE wp_posts set post_modified_gmt='0001-01-01 00:00:00' where post_modified_gmt < '0001-01-01 00:00:00'");
        var rowsAffected2 = await _wpContext.Database.ExecuteSqlAsync($"UPDATE wp_posts set post_date_gmt='0001-01-01 00:00:00' where post_date_gmt < '0001-01-01 00:00:00'");

        return rowsAffected1 + rowsAffected2;
    }

    /// <summary>
    /// Import the tags
    /// </summary>
    private async Task ImportTagsAsync()
    {
        var wpTerms = await _wpContext.WpTerms
            .Where(t => t.WpTermTaxonomies.Any(tt => tt.Taxonomy == "post_tag"))
            .Include(t => t.WpTermTaxonomies)
            .ToListAsync();

        // Add the tags to the database
        foreach (var wpTerm in wpTerms)
        {
            var taxonomy = wpTerm.WpTermTaxonomies.First(tt => tt.Taxonomy == "post_tag");
            var tag = new Tag
            {
                Name = wpTerm.Name,
                Slug = wpTerm.Slug,
                Count = (int)taxonomy.Count
            };

            TryValidate(tag);
            _blogContext.Tag.Add(tag);
        }
        await _blogContext.SaveChangesAsync();

        // Then look for the parents, if they exist
        foreach (var wpTerm in wpTerms.Where(x => x.WpTermTaxonomies.Any(x => x.Parent > 0)))
        {
            foreach (var wpTaxonomy in wpTerm.WpTermTaxonomies.Where(y => y.Parent > 0))
            {
                var parentWpTerm = wpTerms.First(x => x.TermId == wpTaxonomy.Parent);
                var blogTag = await _blogContext.Tag.FirstAsync(x => x.Slug == wpTerm.Slug);
                var blogTagParent = await _blogContext.Tag.FirstAsync(x => x.Slug == parentWpTerm.Slug);
                blogTag.ParentId = blogTagParent.Id;

                TryValidate(blogTag);
                _blogContext.Tag.Update(blogTag);
            }
        }
        await _blogContext.SaveChangesAsync();
    }

    /// <summary>
    /// Import the categories
    /// </summary>
    private async Task ImportCategoriesAsync()
    {
        var wpTerms = await _wpContext.WpTerms
            .Where(t => t.WpTermTaxonomies.Any(tt => tt.Taxonomy == "category"))
            .Include(t => t.WpTermTaxonomies)
            .ToListAsync();

        // First add to the database
        foreach (var wpTerm in wpTerms)
        {
            var taxonomy = wpTerm.WpTermTaxonomies.First(tt => tt.Taxonomy == "category");
            var category = new Category
            {
                Name = wpTerm.Name,
                Slug = wpTerm.Slug,
                Description = taxonomy.Description,
            };

            TryValidate(category);
            _blogContext.Category.Add(category);
        }
        await _blogContext.SaveChangesAsync();

        // Then add the parents
        foreach (var wpTerm in wpTerms.Where(x => x.WpTermTaxonomies.Any(x => x.Parent > 0)))
        {
            foreach (var wpTaxonomy in wpTerm.WpTermTaxonomies.Where(y => y.Parent > 0))
            {
                var parentWpTerm = wpTerms.First(x => x.TermId == wpTaxonomy.Parent);
                var blogCategory = await _blogContext.Category.FirstAsync(x => x.Slug == wpTerm.Slug);
                var blogTagParent = await _blogContext.Category.FirstAsync(x => x.Slug == parentWpTerm.Slug);
                blogCategory.ParentId = blogTagParent.Id;

                TryValidate(blogCategory);
                _blogContext.Category.Update(blogCategory);
            }
        }
        await _blogContext.SaveChangesAsync();
    }

    /// <summary>
    /// Import the attachments
    /// </summary>
    private async Task ImportAttachmentsAsync()
    {
        var wpPosts = _wpContext.WpPosts
            .Where(p => p.PostType == "attachment")
            .Include(p => p.PostMeta);

        if (_storageUtils.GetFileLocation == FileLocation.Local)
            Console.WriteLine("Importing attachments...\n\nPlease move the imported attachments to their path.  They will be imported in:\n" +
                Path.Combine(_storageUtils.GetBasePath, Globals.StorageContainerNameAttachments) + "\n\n");

        foreach (var wpPost in wpPosts)
        {
            //Examples:
            // meta_value (the file in the filesystem) -                      2021/09/business-job-interview-concept-scaled.jpg
            // post_name (not used) -                                                 business-job-interview-concept-2
            // guid (the url) - https://blog.adecco.com.mx/wp-content/uploads/2021/09/business-job-interview-concept.jpg

            // The file location is in the post_meta
            var metaValue = wpPost.PostMeta.First(x => x.MetaKey == "_wp_attached_file").MetaValue ?? throw new InvalidOperationException("MetaValue is null");

            // The file was too big and wordpress scaled it down to 2560px width
            var isScaled = ScaledImageRegex().IsMatch(metaValue);

            // Convert the file path to the appropriate one of our operative system
            var originalFileRelativePath = metaValue.Replace('/', Path.DirectorySeparatorChar);

            // If the file was scaled by wordpress, use that one as our new "original"
            var finalFileRelativePath = isScaled ? originalFileRelativePath.Replace("-scaled.", ".") : originalFileRelativePath;
            finalFileRelativePath = FixGuid(finalFileRelativePath);

            // Get the description of the file
            // Note, more info (sizes, camera info, keywords etc) is available in the PostMeta with key _wp_attachment_metadata, stored as a PHP serialized object
            string? description = null;
            string? title = null;
            var meta = wpPost.PostMeta.FirstOrDefault(x => x.MetaKey == "_wp_attachment_metadata")?.MetaValue;
            if (meta != null
                && PhpSerialization.Deserialize(meta) is Dictionary<object, object> phpObject
                && phpObject.ContainsKey("image_meta")
                && phpObject["image_meta"] is Dictionary<object, object> image_meta)
            {
                if (image_meta["caption"] != null && image_meta["caption"].ToString() != "")
                {
                    description = image_meta["caption"].ToString();
                }

                if (image_meta["title"] != null && image_meta["title"].ToString() != "")
                {
                    title = image_meta["title"].ToString();
                }
            }

            var originalFileFullPath = Path.Combine(oldUploadsPath, originalFileRelativePath);
            if (!File.Exists(originalFileFullPath))
            {
                Console.WriteLine($"File not found {originalFileFullPath}.");
                continue;
                //throw new IOException($"File not found {originalFileFullPath}.");
            }

            using var fileStream = File.OpenRead(originalFileFullPath);
            var storedFile = await _storageUtils.UploadFileAsync(fileStream, wpPost.PostMimeType, Globals.StorageContainerNameAttachments);

            // Currently we're only storing locally the imported files
            var attachment = new Attachment
            {
                File = storedFile.Name,
                Container = storedFile.Container,
                Location = storedFile.Location,

                Created = wpPost.PostDate,
                CreatedById = await GetOrCreateUserAsync(wpPost.PostAuthor),
                MimeType = wpPost.PostMimeType,
                Description = description?.Trim(),
                Title = title?.Trim(),
                Alt = wpPost.PostMeta.FirstOrDefault(x => x.MetaKey == "_wp_attachment_image_alt")?.MetaValue,

                OriginalFilename = Path.GetFileName(finalFileRelativePath),
                Slug = "/uploads/" + FixGuid(isScaled ? metaValue.Replace("-scaled.", ".") : metaValue),
            };

            // If the attachment is an image, create a thumbnail
            if (MDGlobals.AcceptedImages.Contains(wpPost.PostMimeType))
            {
                fileStream.Position = 0;
                fileStream.Seek(0, SeekOrigin.Begin);

                // Create the thumbnail
                var image = ImageUtils.CreateThumbnail(fileStream, wpPost.PostMimeType, MDGlobals.ThumbWidth);
                if (image != null)
                {
                    var thumbnail = await _storageUtils.UploadFileAsync(image, wpPost.PostMimeType, attachment.Container);
                    attachment.ThumbFile = thumbnail.Name;
                    attachment.ThumbContainer = thumbnail.Container;
                }
            }

            TryValidate(attachment);
            _blogContext.Attachment.Add(attachment);
        }
        await _blogContext.SaveChangesAsync();
    }

    /// <summary>
    /// Import the posts
    /// </summary>
    private async Task ImportPostsAsync()
    {
        // Only import the published posts
        var wpQuery = _wpContext.WpPosts
            .Include(p => p.PostMeta)
            .Include(p => p.TermRelationships).ThenInclude(tr => tr.TermTaxonomy).ThenInclude(t => t!.Term)
            .Where(p => p.PostType == "post" && p.PostStatus == "publish");

        if (_configuration.GetValue("WordpressImport:SkipEmptyPosts", true))
        {
            wpQuery = wpQuery.Where(p => !string.IsNullOrEmpty(p.PostContent));
        }

        var wpPosts = await wpQuery.ToListAsync();
        var metaKeysToImport = _configuration.GetSection("WordpressImport:PostMetas").Get<List<string>>();

        foreach (var wpPost in wpPosts)
        {
            // Cleanup texts
            wpPost.PostTitle = RemoveStrangeCharacters(wpPost.PostTitle);
            wpPost.PostContent = RemoveStrangeCharacters(wpPost.PostContent);
            wpPost.PostName = RemoveStrangeCharacters(wpPost.PostName);
            wpPost.PostExcerpt = RemoveStrangeCharacters(wpPost.PostExcerpt);

            if (string.IsNullOrWhiteSpace(wpPost.PostContent))
            {
                wpPost.PostContent = "";
            }

            var post = new Post
            {
                Title = wpPost.PostTitle,
                Summary = string.IsNullOrWhiteSpace(wpPost.PostExcerpt) ? CreateSummary(wpPost.PostContent) : wpPost.PostExcerpt,
                Content = FixGuid(wpPost.PostContent),
                Slug = wpPost.PostName,
                Created = wpPost.PostDate,
                Published = wpPost.PostModified,
                CreatedById = await GetOrCreateUserAsync(wpPost.PostAuthor),
                Modified = wpPost.PostModified,
                Status = PostStatus.Published,
                Tags = [],
                Categories = [],
                Metadata = [],
            };

            if (metaKeysToImport != null)
            {
                foreach (var metadata in wpPost.PostMeta)
                {
                    if (metadata.MetaKey == null) continue;
                    if (metaKeysToImport.Contains(metadata.MetaKey))
                    {
                        if (!post.Metadata.Any(x => x.Key == metadata.MetaKey && x.Value == metadata.MetaValue))
                            post.Metadata.Add(new() { Key = metadata.MetaKey, Value = metadata.MetaValue });
                    }
                }
            }

            // Get the thumbnail from the postmeta info of the post
            var metaImage = wpPost.PostMeta.FirstOrDefault(x => x.MetaKey == "_thumbnail_id");
            if (metaImage != null && ulong.TryParse(metaImage.MetaValue, out var imageId))
            {
                // Get the thumbnail information, which is stored in another post (of type attachment)
                var wpImage = await _wpContext.WpPosts.FirstOrDefaultAsync(x => x.Id == imageId);
                if (wpImage != null)
                {
                    // We should have imported previously all the attachments to our new blog, so we find the post thumbnail in the already imported database
                    var image = await _blogContext.Attachment.FirstOrDefaultAsync(x => x.Slug == FixGuid(wpImage.Guid));
                    if (image != null)
                    {
                        // And attach it as the featured image of the post
                        post.FeaturedImageId = image.Id;
                    }
                    else
                    {
                        Console.WriteLine($"Cannot find attachment with guid {wpImage.Guid}.");
                    }
                }
                else
                {
                    Console.WriteLine($"Cannot find the thumbnail post with ID {imageId}.");
                }
            }
            else
            {
                Console.WriteLine($"Post {wpPost.Id} has no thumbnail");
            }

            // Get the post categories and tags
            foreach (var termRelationship in wpPost.TermRelationships)
            {
                var termTaxonmy = termRelationship.TermTaxonomy;
                if (termTaxonmy == null || termTaxonmy.Term == null)
                {
                    Console.WriteLine($"Post {wpPost.Id} taxonomy is empty");
                    continue;
                }

                if (termTaxonmy.Taxonomy == "category")
                {
                    var category = await _blogContext.Category.FirstAsync(x => x.Slug == termTaxonmy.Term.Slug);
                    if (category.Name == _configuration["WordpressImport:FixedCategory"] && _configuration["WordpressImport:FixedCategory"] != null)
                    {
                        post.Fixed = true;
                    }

                    TryValidate(category);
                    post.Categories.Add(category);
                }
                else if (termTaxonmy.Taxonomy == "post_tag")
                {
                    var tag = await _blogContext.Tag.FirstAsync(x => x.Slug == termTaxonmy.Term.Slug);

                    TryValidate(tag);
                    post.Tags.Add(tag);
                }
            }

            TryValidate(post);
            _blogContext.Post.Add(post);
        }
        await _blogContext.SaveChangesAsync();
    }

    private async Task<string> GetOrCreateUserAsync(ulong id)
    {
        if (userList.TryGetValue(id, out string? value))
            return value;

        var wpUser = await _wpContext.WpUsers
              .Include(p => p.UserMeta)
              .FirstAsync(p => p.Id == id);

        if (string.IsNullOrWhiteSpace(wpUser.UserEmail))
        {
            Console.WriteLine($"User {wpUser.DisplayName} (Id: {wpUser.Id}) has no email.");
            throw new InvalidOperationException("User without email");
        }

        var blogUser = await _blogContext.ApplicationUser.FirstOrDefaultAsync(x => x.UserName == wpUser.UserEmail);
        if (blogUser != null)
        {
            userList.Add(id, blogUser.Id);
            return blogUser.Id;
        }

        var user = new ApplicationUser
        {
            Firstname = wpUser.GetMeta("first_name") ?? throw new InvalidOperationException("first_name meta not found in user"),
            Lastname = wpUser.GetMeta("last_name") ?? throw new InvalidOperationException("last_name meta not found in user"),
            Approved = true,
            EmailConfirmed = true,
            Position = wpUser.GetMeta("position"),
            Bio = wpUser.GetMeta("description"),
            Twitter = wpUser.GetMeta("twitter_url"),
            Linkedin = wpUser.GetMeta("linkedin_url"),
            DisplayName = wpUser.DisplayName,
            // Company
            // PhoneNumber
        };

        await _userStore.SetUserNameAsync(user, wpUser.UserEmail, CancellationToken.None);
        await _emailStore.SetEmailAsync(user, wpUser.UserEmail, CancellationToken.None);

        var result = await _userManager.CreateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Couldn't create new user");
        }

        await _userManager.AddToRoleAsync(user, Globals.RoleSupervisor);

        userList.Add(id, user.Id);
        return user.Id;
    }

    /// <summary>
    /// Return the Guid (unique URI) for an attachment edited to meet our needs, for example with the first part of the URL
    /// </summary>
    private string FixGuid(string value)
    {
        foreach (var site in _sitesToImport)
        {
            value = value.Replace($"https://{site}/wp-content", "");
            value = value.Replace($"http://{site}/wp-content", "");
        }

        // Remove unwanted characters
        value = value.Replace("–", "-"); // Dash converts to hyphen
        value = value.Replace("@", "-"); // @ converts to hyphen

        return value;
    }

    /// <summary>
    /// Regex that matches scaled attachments image files.
    /// </summary>
    [GeneratedRegex(@"-scaled\.(jpg|jpeg|png|gif)$")]
    private static partial Regex ScaledImageRegex();

    /// <summary>
    /// Regex to match different space character types
    /// </summary>
    [GeneratedRegex(@"[\s]")]
    private static partial Regex OneSpacheChar();

    /// <summary>
    /// Regex to match HTML non breaking spaces
    /// </summary>
    [GeneratedRegex(@"&nbsp;")]
    private static partial Regex NBSpace();

    /// <summary>
    /// Regex to match multiple sequences of spaces
    /// </summary>
    [GeneratedRegex(@"[\s]{2,}")]
    private static partial Regex ManySpaceChars();
}