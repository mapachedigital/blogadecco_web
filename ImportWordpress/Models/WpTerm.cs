// Copyright (c) 2021, Mapache Digital
// Version: 1.0.0
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

namespace ImportWordpress.Models;

public partial class WpTerm
{
    public ulong TermId { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public long TermGroup { get; set; }

    public List<WpTermTaxonomy> WpTermTaxonomies { get; } = [];
}
