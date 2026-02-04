// Copyright (c) 2021, Mapache Digital
// Version: 1.4
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using MDWidgets.Utils.ModelAttributes;
using System.ComponentModel.DataAnnotations;

namespace BlogAdecco.Areas.Admin.Models.CategoryViewModels;

/// <summary>
/// Model for storing a category
/// </summary>
public class CategoryCreateViewModel
{
    public int Id { get; set; }

    /// <summary>
    /// The name of the category
    /// </summary>
    [SanitizeHtml]
    [StringLength(200, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// The description of the category
    /// </summary>
    [SanitizeHtml]
    [StringLength(300, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; } = default!;

    /// <summary>
    /// The parent category, if any
    /// </summary>
    [Display(Name = "Parent")]
    public int? ParentId { get; set; }
}
