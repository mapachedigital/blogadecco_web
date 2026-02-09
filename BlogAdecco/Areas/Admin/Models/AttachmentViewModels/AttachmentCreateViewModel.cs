// Copyright (c) 2025, Mapache Digital
// Version: 1.0
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using MDWidgets.Utils.ModelAttributes;
using System.ComponentModel.DataAnnotations;

namespace BlogAdecco.Areas.Admin.Models.AttachmentViewModels;

public class AttachmentCreateViewModel
{
    public int Id { get; set; }

    /*
    /// <summary>
    /// The featured image (thumbnail) for this document
    /// </summary>
    [DataType(DataType.Upload)]
    [Display(Name = "Document")]
    [Required(ErrorMessage = "The '{0}' field is required.")]
    public IFormFile File { get; set; } = default!;
    */

    /// <summary>
    /// The alternative text of the attachment
    /// </summary>
    [SanitizeHtml]
    [StringLength(300, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Display(Name = "Alt")]
    public string? Alt { get; set; } = default!;

    /// <summary>
    /// The title of the attachment
    /// </summary>
    [SanitizeHtml]
    [StringLength(300, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Display(Name = "Title")]
    public string? Title { get; set; } = default!;

    /// <summary>
    /// The description of the attachment
    /// </summary>
    [SanitizeHtml]
    [StringLength(300, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; } = default!;

    /// <summary>
    /// The attachment uploaded file
    /// </summary>
    [DataType(DataType.Upload)]
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "File")]
    public IFormFile File { get; set; } = default!;
}
