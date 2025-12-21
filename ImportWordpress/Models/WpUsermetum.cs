// Copyright (c) 2021, Mapache Digital
// Version: 1.0.0
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

namespace ImportWordpress.Models;

public partial class WpUsermetum
{
    public ulong UmetaId { get; set; }

    public ulong UserId { get; set; }

    public string? MetaKey { get; set; }

    public string? MetaValue { get; set; }
}
