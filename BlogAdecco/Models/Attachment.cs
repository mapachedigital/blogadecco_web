// Copyright (c) 2021, Mapache Digital
// Version: 1.4
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using MDWidgets.Utils;
using System.ComponentModel.DataAnnotations;

namespace BlogAdecco.Models;

/// <summary>
/// Model for storing an uploaded photo or document
/// </summary>
public class Attachment
{
    public Attachment()
    {
        File = string.Empty;
        Container = string.Empty;
        OriginalFilename = string.Empty;
        MimeType = string.Empty;
    }

    public int Id { get; set; }

    /// <summary>
    /// The path of the file or blob name as stored in the server
    /// </summary>
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Filename")]
    [StringLength(256, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    public string File { get; set; }

    /// <summary>
    /// The container or directory where the blob or file is stored
    /// </summary>
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Container")]
    [RegularExpression(@"^[a-z0-9](?!.*--)[a-z0-9-]{1,61}[a-z0-9]$", ErrorMessage = "Invalid characters")]
    [StringLength(63, MinimumLength = 3, ErrorMessage = "The '{0}' field must be at least {2} and at max {1} characters long.")]
    public string Container { get; set; }

    /// <summary>
    /// The path of the file or blob name as stored in the server
    /// </summary>
    [Display(Name = "Thumbnail Filename")]
    [StringLength(256, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    public string? ThumbFile { get; set; }

    /// <summary>
    /// The container or directory where the blob or file is stored
    /// </summary>
    [Display(Name = "Container")]
    [RegularExpression(@"^[a-z0-9](?!.*--)[a-z0-9-]{1,61}[a-z0-9]$", ErrorMessage = "Invalid characters")]
    [StringLength(63, MinimumLength = 3, ErrorMessage = "The '{0}' field must be at least {2} and at max {1} characters long.")]
    public string? ThumbContainer { get; set; }

    /// <summary>
    /// The location of this file: cloud or local
    /// </summary>
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Location")]
    public FileLocation Location { get; set; }

    /// <summary>
    /// The original name of the file.
    /// </summary>
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Original Name")]
    [StringLength(256, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    public string OriginalFilename { get; set; }

    /// <summary>
    /// The content-type of the file
    /// </summary>
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "MIME Type")]
    [StringLength(125, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    public string MimeType { get; set; }

    /// <summary>
    /// The time the file was uploaded
    /// </summary>
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Created")]
    public DateTime Created { get; set; }

    /// <summary>
    /// The user who created the attachment.
    /// </summary>
    [Display(Name = "Created By")]
    public string? CreatedById { get; set; }

    /// <summary>
    /// The user who created the attachment.
    /// </summary>
    [Display(Name = "Created By")]
    public ApplicationUser? CreatedBy { get; set; }

    /// <summary>
    ///  The posts using this attachment
    /// </summary>
    [Display(Name = "Posts")]
    public List<Post> Posts { get; set; } = [];
}

/// <summary>
/// The content disposition of an attachment when downloaded
/// </summary>
public enum ContentDisposition { Inline, Attachment }

