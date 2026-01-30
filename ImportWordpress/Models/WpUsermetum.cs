// Copyright (c) 2021, Mapache Digital
// Version: 1.1.0
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using System.ComponentModel.DataAnnotations.Schema;

namespace ImportWordpress.Models;

public partial class WpUsermetum
{
    public ulong UmetaId { get; set; }

    [ForeignKey(nameof(User))]
    public ulong UserId { get; set; }

    public WpUser? User { get; set; } = null!;

    public string? MetaKey { get; set; }

    public string? MetaValue { get; set; }
}
