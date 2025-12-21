// Copyright (c) 2021, Mapache Digital
// Version: 1.0.0
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImportWordpress.Models;

[PrimaryKey(nameof(TermTaxonomyId))]
public partial class WpTermTaxonomy
{
    public ulong TermTaxonomyId { get; set; }

    [ForeignKey(nameof(Term))]
    public ulong TermId { get; set; }

    public WpTerm? Term { get; set; } = null!;

    public string Taxonomy { get; set; } = null!;

    public string Description { get; set; } = null!;

    public ulong Parent { get; set; }

    public long Count { get; set; }
}
