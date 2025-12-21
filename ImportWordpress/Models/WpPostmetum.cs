// Copyright (c) 2021, Mapache Digital
// Version: 1.0.0
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using System.ComponentModel.DataAnnotations.Schema;

namespace ImportWordpress.Models;

public partial class WpPostmetum
{
    public ulong MetaId { get; set; }

    [ForeignKey(nameof(Post))]
    public ulong PostId { get; set; }

    public WpPost? Post { get; set; } = null!;

    public string? MetaKey { get; set; }

    public string? MetaValue { get; set; }
}
