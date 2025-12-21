// Copyright (c) 2021, Mapache Digital
// Version: 1.4
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco.Data;
using MDWidgets.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace BlogAdecco.Pages;

public partial class UploadsModel(ApplicationDbContext _context, IStorageUtils _storageUtils) : PageModel
{
    [FromRoute]
    public int Year { get; set; }

    [FromRoute]
    public int Month { get; set; }

    [FromRoute]
    public string Path { get; set; } = default!;


    public async Task<IActionResult> OnGetAsync()
    {
        // Since the database was imported from WordPress, it contains several 'derived' URLs for different image sizes.
        // However, as we only have one source URL, we need to strip these suffixes to normalize the paths.
        Path = FixPath(Path);

        var attachment = await _context.Attachment.FirstOrDefaultAsync(x => x.Guid == $"/uploads/{Year:D2}/{Month:D2}/{Path}");

        if (attachment == null)
        {
            return NotFound();
        }

        var upload = await _storageUtils.GetFileAsync(attachment.File, attachment.Container, attachment.Location);
        if (upload == null)
        {
            return NotFound();
        }

        using var stream = upload.Value.Item1;

        var mimeType = upload.Value.Item2;
        var data = stream.ToArray();

        Response.Headers[HeaderNames.ContentDisposition] = new ContentDispositionHeaderValue("inline") { FileName = attachment.OriginalFilename, }.ToString();
        return File(data, mimeType);
    }

    /// <summary>
    /// Remove extra information about the image size on the URL
    /// </summary>
    private static string FixPath(string path)
    {
        path = ScaledResolutionImageRegex().Replace(path, ".$2");
        return path;
    }

    /// <summary>
    /// Regex to remove image size information from the URL, for example http://example.com/image-fill-path-1024x768.jpg
    /// </summary>
    [GeneratedRegex(@"-(scaled|[0-9]{3,4}x[0-9]{2,4})\.(jpeg|jpg|png|gif)$")]
    private static partial Regex ScaledResolutionImageRegex();
}
