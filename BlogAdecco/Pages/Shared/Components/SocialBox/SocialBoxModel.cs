// Copyright (c) 2021, Mapache Digital
// Version: 1.0.0
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

namespace BlogAdecco.Pages.Shared.Components.SocialBox;

/// <summary>
/// Model for the SocialBox view component.
/// </summary>
public class SocialBoxModel
{
    public string FacebookShareUrl { get; set; } = string.Empty;
    public string TwitterShareUrl { get; set; } = string.Empty;
    public string LinkedInShareUrl { get; set; } = string.Empty;
    public string WhatsappShareUrl { get; set; } = string.Empty;
}