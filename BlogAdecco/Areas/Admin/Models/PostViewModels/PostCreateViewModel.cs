// Copyright (c) 2021, Mapache Digital
// Version: 1.4
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco.Models;
using MDWidgets.Utils.ModelAttributes;
using System.ComponentModel.DataAnnotations;

namespace BlogAdecco.Areas.Admin.Models.PostViewModels;

/// <summary>
/// Model for storing a category
/// </summary>
public class PostCreateViewModel
{
    public int Id { get; set; }

    /// <summary>
    /// The title of the post
    /// </summary>
    [SanitizeHtml]
    [StringLength(200, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = default!;

    /// <summary>
    /// The title of the post
    /// </summary>
    [SanitizeHtml]
    [StringLength(300, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Display(Name = "Summary")]
    public string? Summary { get; set; } = default!;

    /// <summary>
    /// The HTML content of the posting
    /// </summary>
    [SanitizeHtml(false)]
    [DataType(DataType.Html)]
    [Required(AllowEmptyStrings = true, ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Content")]
    public string Content { get; set; } = default!;

    /// <summary>
    /// The featured image (thumbnail) for this post
    /// </summary>
    [DataType(DataType.Upload)]
    [Display(Name = "Image")]
    public IFormFile? FeaturedImage { get; set; }

    /// <summary>
    /// The active / draft status of the post
    /// </summary>
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Status")]
    public PostStatus Status { get; set; }

    /// <summary>
    /// A list of categories or tags for the post
    /// </summary>
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Categories")]
    public List<int> CategoryId { get; set; } = [];

    /// <summary>
    /// A list of categories or tags for the post
    /// </summary>
    [Display(Name = "Tags")]
    public List<int>? TagId { get; set; } = [];

    /// <summary>
    /// Indicates that the post must be shown first
    /// </summary>
    [Display(Name = "Fixed")]
    public bool Fixed { get; set; } = false;
}
