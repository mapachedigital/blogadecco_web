// Copyright (c) 2021, Mapache Digital
// Version: 1.4
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using MDWidgets.Utils.ModelAttributes;
using System.ComponentModel.DataAnnotations;

namespace BlogAdecco.Areas.Admin.Models.TagViewModels;

/// <summary>
/// Model for storing a tag
/// </summary>
public class TagCreateViewModel
{
    public int Id { get; set; }

    /// <summary>
    /// The name of the tag
    /// </summary>
    [SanitizeHtml]
    [StringLength(200, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// The parent tag, if any
    /// </summary>
    [Display(Name = "Parent")]
    public int? ParentId { get; set; }
}
