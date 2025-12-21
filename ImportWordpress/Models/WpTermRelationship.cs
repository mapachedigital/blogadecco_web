// Copyright (c) 2021, Mapache Digital
// Version: 1.0.0
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImportWordpress.Models;

[PrimaryKey(nameof(ObjectId), nameof(TermTaxonomyId))]
public partial class WpTermRelationship
{
    [ForeignKey(nameof(Post))]
    public ulong ObjectId { get; set; }

    public WpPost? Post { get; set; }

    [ForeignKey(nameof(TermTaxonomyId))]
    public ulong TermTaxonomyId { get; set; }

    public WpTermTaxonomy? TermTaxonomy { get; set; }

    public int TermOrder { get; set; }
}
