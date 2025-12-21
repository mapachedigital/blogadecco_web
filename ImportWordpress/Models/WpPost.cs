// Copyright (c) 2021, Mapache Digital
// Version: 1.0.0
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

namespace ImportWordpress.Models;

public partial class WpPost
{
    public ulong Id { get; set; }

    public ulong PostAuthor { get; set; }

    public DateTime PostDate { get; set; }

    public DateTime PostDateGmt { get; set; }

    public string PostContent { get; set; } = null!;

    public string PostTitle { get; set; } = null!;

    public string PostExcerpt { get; set; } = null!;

    public string PostStatus { get; set; } = null!;

    public string CommentStatus { get; set; } = null!;

    public string PingStatus { get; set; } = null!;

    public string PostPassword { get; set; } = null!;

    public string PostName { get; set; } = null!;

    public string ToPing { get; set; } = null!;

    public string Pinged { get; set; } = null!;

    public DateTime PostModified { get; set; }

    public DateTime PostModifiedGmt { get; set; }

    public string PostContentFiltered { get; set; } = null!;

    public ulong PostParent { get; set; }

    public string Guid { get; set; } = null!;

    public int MenuOrder { get; set; }

    public string PostType { get; set; } = null!;

    public string PostMimeType { get; set; } = null!;

    public long CommentCount { get; set; }

    public List<WpPostmetum> PostMeta { get; } = [];

    public List<WpTermRelationship> TermRelationships { get; } = [];
}
