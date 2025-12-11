// Copyright (c) 2021, Mapache Digital
// Version: 1.2
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

namespace BlogAdecco.Utils;

/// <summary>
/// Class representing header data for pages.
/// </summary>
public class HeaderData
{
    public string Template { get; set; } = default!;
    public string Subtitle { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public List<string> Breadcrumbs { get; set; } = [];
    public List<HeaderLink> Links1 { get; set; } = [];
    public List<HeaderLink> Links2 { get; set; } = [];
}

/// <summary>
/// Class representing a link in the header.
/// </summary>
public class HeaderLink
{
    public string Title { get; set; } = default!;
    public int Width { get; set; }
    public string? Url { get; set; }
    public bool Enabled { get; set; } = true;

    public string WidthPx => $"{Width}px";
}