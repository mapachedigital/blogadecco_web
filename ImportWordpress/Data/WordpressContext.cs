// Copyright (c) 2021, Mapache Digital
// Version: 1.0.0
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using ImportWordpress.Models;
using Microsoft.EntityFrameworkCore;

namespace ImportWordpress.Data;

public partial class WordpressContext(DbContextOptions<WordpressContext> options) : DbContext(options)
{
    public virtual DbSet<WpComment> WpComments { get; set; }

    public virtual DbSet<WpCommentmetum> WpCommentmeta { get; set; }

    public virtual DbSet<WpLink> WpLinks { get; set; }

    public virtual DbSet<WpOption> WpOptions { get; set; }

    public virtual DbSet<WpPost> WpPosts { get; set; }

    public virtual DbSet<WpPostmetum> WpPostmeta { get; set; }

    public virtual DbSet<WpTerm> WpTerms { get; set; }

    public virtual DbSet<WpTermRelationship> WpTermRelationships { get; set; }

    public virtual DbSet<WpTermTaxonomy> WpTermTaxonomies { get; set; }

    public virtual DbSet<WpTermmetum> WpTermmeta { get; set; }

    public virtual DbSet<WpUser> WpUsers { get; set; }

    public virtual DbSet<WpUsermetum> WpUsermeta { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WpComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PRIMARY");

            entity.ToTable("wp_comments");

            entity.HasIndex(e => new { e.CommentApproved, e.CommentDateGmt }, "comment_approved_date_gmt");

            entity.HasIndex(e => e.CommentAuthorEmail, "comment_author_email");

            entity.HasIndex(e => e.CommentDateGmt, "comment_date_gmt");

            entity.HasIndex(e => e.CommentParent, "comment_parent");

            entity.HasIndex(e => e.CommentPostId, "comment_post_ID");

            entity.Property(e => e.CommentId).HasColumnName("comment_ID");
            entity.Property(e => e.CommentAgent)
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("comment_agent");
            entity.Property(e => e.CommentApproved)
                .HasMaxLength(20)
                .HasDefaultValueSql("'1'")
                .HasColumnName("comment_approved");
            entity.Property(e => e.CommentAuthor)
                .HasColumnType("tinytext")
                .HasColumnName("comment_author");
            entity.Property(e => e.CommentAuthorEmail)
                .HasMaxLength(100)
                .HasDefaultValueSql("''")
                .HasColumnName("comment_author_email");
            entity.Property(e => e.CommentAuthorIp)
                .HasMaxLength(100)
                .HasDefaultValueSql("''")
                .HasColumnName("comment_author_IP");
            entity.Property(e => e.CommentAuthorUrl)
                .HasMaxLength(200)
                .HasDefaultValueSql("''")
                .HasColumnName("comment_author_url");
            entity.Property(e => e.CommentContent)
                .HasColumnType("text")
                .HasColumnName("comment_content");
            entity.Property(e => e.CommentDate)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime")
                .HasColumnName("comment_date");
            entity.Property(e => e.CommentDateGmt)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime")
                .HasColumnName("comment_date_gmt");
            entity.Property(e => e.CommentKarma).HasColumnName("comment_karma");
            entity.Property(e => e.CommentParent).HasColumnName("comment_parent");
            entity.Property(e => e.CommentPostId).HasColumnName("comment_post_ID");
            entity.Property(e => e.CommentType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'comment'")
                .HasColumnName("comment_type");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<WpCommentmetum>(entity =>
        {
            entity.HasKey(e => e.MetaId).HasName("PRIMARY");

            entity.ToTable("wp_commentmeta");

            entity.HasIndex(e => e.CommentId, "comment_id");

            entity.HasIndex(e => e.MetaKey, "meta_key");

            entity.Property(e => e.MetaId).HasColumnName("meta_id");
            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.MetaKey).HasColumnName("meta_key");
            entity.Property(e => e.MetaValue).HasColumnName("meta_value");
        });

        modelBuilder.Entity<WpLink>(entity =>
        {
            entity.HasKey(e => e.LinkId).HasName("PRIMARY");

            entity.ToTable("wp_links");

            entity.HasIndex(e => e.LinkVisible, "link_visible");

            entity.Property(e => e.LinkId).HasColumnName("link_id");
            entity.Property(e => e.LinkDescription)
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("link_description");
            entity.Property(e => e.LinkImage)
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("link_image");
            entity.Property(e => e.LinkName)
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("link_name");
            entity.Property(e => e.LinkNotes)
                .HasColumnType("mediumtext")
                .HasColumnName("link_notes");
            entity.Property(e => e.LinkOwner)
                .HasDefaultValueSql("'1'")
                .HasColumnName("link_owner");
            entity.Property(e => e.LinkRating).HasColumnName("link_rating");
            entity.Property(e => e.LinkRel)
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("link_rel");
            entity.Property(e => e.LinkRss)
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("link_rss");
            entity.Property(e => e.LinkTarget)
                .HasMaxLength(25)
                .HasDefaultValueSql("''")
                .HasColumnName("link_target");
            entity.Property(e => e.LinkUpdated)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime")
                .HasColumnName("link_updated");
            entity.Property(e => e.LinkUrl)
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("link_url");
            entity.Property(e => e.LinkVisible)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Y'")
                .HasColumnName("link_visible");
        });

        modelBuilder.Entity<WpOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PRIMARY");

            entity.ToTable("wp_options");

            entity.HasIndex(e => e.Autoload, "autoload");

            entity.HasIndex(e => e.OptionName, "option_name").IsUnique();

            entity.Property(e => e.OptionId).HasColumnName("option_id");
            entity.Property(e => e.Autoload)
                .HasMaxLength(20)
                .HasDefaultValueSql("'yes'")
                .HasColumnName("autoload");
            entity.Property(e => e.OptionName)
                .HasMaxLength(191)
                .HasDefaultValueSql("''")
                .HasColumnName("option_name");
            entity.Property(e => e.OptionValue).HasColumnName("option_value");
        });

        modelBuilder.Entity<WpPost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("wp_posts");

            entity.HasIndex(e => e.PostAuthor, "post_author");

            entity.HasIndex(e => e.PostName, "post_name");

            entity.HasIndex(e => e.PostParent, "post_parent");

            entity.HasIndex(e => new { e.PostType, e.PostStatus, e.PostDate, e.Id }, "type_status_date");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CommentCount).HasColumnName("comment_count");
            entity.Property(e => e.CommentStatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'open'")
                .HasColumnName("comment_status");
            entity.Property(e => e.Guid)
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("guid");
            entity.Property(e => e.MenuOrder).HasColumnName("menu_order");
            entity.Property(e => e.PingStatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'open'")
                .HasColumnName("ping_status");
            entity.Property(e => e.Pinged)
                .HasColumnType("text")
                .HasColumnName("pinged");
            entity.Property(e => e.PostAuthor).HasColumnName("post_author");
            entity.Property(e => e.PostContent).HasColumnName("post_content");
            entity.Property(e => e.PostContentFiltered).HasColumnName("post_content_filtered");
            entity.Property(e => e.PostDate)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime")
                .HasColumnName("post_date");
            entity.Property(e => e.PostDateGmt)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime")
                .HasColumnName("post_date_gmt");
            entity.Property(e => e.PostExcerpt)
                .HasColumnType("text")
                .HasColumnName("post_excerpt");
            entity.Property(e => e.PostMimeType)
                .HasMaxLength(100)
                .HasDefaultValueSql("''")
                .HasColumnName("post_mime_type");
            entity.Property(e => e.PostModified)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime")
                .HasColumnName("post_modified");
            entity.Property(e => e.PostModifiedGmt)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime")
                .HasColumnName("post_modified_gmt");
            entity.Property(e => e.PostName)
                .HasMaxLength(200)
                .HasDefaultValueSql("''")
                .HasColumnName("post_name");
            entity.Property(e => e.PostParent).HasColumnName("post_parent");
            entity.Property(e => e.PostPassword)
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("post_password");
            entity.Property(e => e.PostStatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'publish'")
                .HasColumnName("post_status");
            entity.Property(e => e.PostTitle)
                .HasColumnType("text")
                .HasColumnName("post_title");
            entity.Property(e => e.PostType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'post'")
                .HasColumnName("post_type");
            entity.Property(e => e.ToPing)
                .HasColumnType("text")
                .HasColumnName("to_ping");
        });

        modelBuilder.Entity<WpPostmetum>(entity =>
        {
            entity.HasKey(e => e.MetaId).HasName("PRIMARY");

            entity.ToTable("wp_postmeta");

            entity.HasIndex(e => e.MetaKey, "meta_key");

            entity.HasIndex(e => e.PostId, "post_id");

            entity.Property(e => e.MetaId).HasColumnName("meta_id");
            entity.Property(e => e.MetaKey).HasColumnName("meta_key");
            entity.Property(e => e.MetaValue).HasColumnName("meta_value");
            entity.Property(e => e.PostId).HasColumnName("post_id");
        });

        modelBuilder.Entity<WpTerm>(entity =>
        {
            entity.HasKey(e => e.TermId).HasName("PRIMARY");

            entity.ToTable("wp_terms");

            entity.HasIndex(e => e.Name, "name");

            entity.HasIndex(e => e.Slug, "slug");

            entity.Property(e => e.TermId).HasColumnName("term_id");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasDefaultValueSql("''")
                .HasColumnName("name");
            entity.Property(e => e.Slug)
                .HasMaxLength(200)
                .HasDefaultValueSql("''")
                .HasColumnName("slug");
            entity.Property(e => e.TermGroup).HasColumnName("term_group");
        });

        modelBuilder.Entity<WpTermRelationship>(entity =>
        {
            entity.HasKey(e => new { e.ObjectId, e.TermTaxonomyId }).HasName("PRIMARY");

            entity.ToTable("wp_term_relationships");

            entity.HasIndex(e => e.TermTaxonomyId, "term_taxonomy_id");

            entity.Property(e => e.ObjectId).HasColumnName("object_id");
            entity.Property(e => e.TermTaxonomyId).HasColumnName("term_taxonomy_id");
            entity.Property(e => e.TermOrder).HasColumnName("term_order");
        });

        modelBuilder.Entity<WpTermTaxonomy>(entity =>
        {
            entity.HasKey(e => e.TermTaxonomyId).HasName("PRIMARY");

            entity.ToTable("wp_term_taxonomy");

            entity.HasIndex(e => e.Taxonomy, "taxonomy");

            entity.HasIndex(e => new { e.TermId, e.Taxonomy }, "term_id_taxonomy").IsUnique();

            entity.Property(e => e.TermTaxonomyId).HasColumnName("term_taxonomy_id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Parent).HasColumnName("parent");
            entity.Property(e => e.Taxonomy)
                .HasMaxLength(32)
                .HasDefaultValueSql("''")
                .HasColumnName("taxonomy");
            entity.Property(e => e.TermId).HasColumnName("term_id");
        });

        modelBuilder.Entity<WpTermmetum>(entity =>
        {
            entity.HasKey(e => e.MetaId).HasName("PRIMARY");

            entity.ToTable("wp_termmeta");

            entity.HasIndex(e => e.MetaKey, "meta_key");

            entity.HasIndex(e => e.TermId, "term_id");

            entity.Property(e => e.MetaId).HasColumnName("meta_id");
            entity.Property(e => e.MetaKey).HasColumnName("meta_key");
            entity.Property(e => e.MetaValue).HasColumnName("meta_value");
            entity.Property(e => e.TermId).HasColumnName("term_id");
        });

        modelBuilder.Entity<WpUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("wp_users");

            entity.HasIndex(e => e.UserEmail, "user_email");

            entity.HasIndex(e => e.UserLogin, "user_login_key");

            entity.HasIndex(e => e.UserNicename, "user_nicename");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(250)
                .HasDefaultValueSql("''")
                .HasColumnName("display_name");
            entity.Property(e => e.UserActivationKey)
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("user_activation_key");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .HasDefaultValueSql("''")
                .HasColumnName("user_email");
            entity.Property(e => e.UserLogin)
                .HasMaxLength(60)
                .HasDefaultValueSql("''")
                .HasColumnName("user_login");
            entity.Property(e => e.UserNicename)
                .HasMaxLength(50)
                .HasDefaultValueSql("''")
                .HasColumnName("user_nicename");
            entity.Property(e => e.UserPass)
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("user_pass");
            entity.Property(e => e.UserRegistered)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime")
                .HasColumnName("user_registered");
            entity.Property(e => e.UserStatus).HasColumnName("user_status");
            entity.Property(e => e.UserUrl)
                .HasMaxLength(100)
                .HasDefaultValueSql("''")
                .HasColumnName("user_url");
        });

        modelBuilder.Entity<WpUsermetum>(entity =>
        {
            entity.HasKey(e => e.UmetaId).HasName("PRIMARY");

            entity.ToTable("wp_usermeta");

            entity.HasIndex(e => e.MetaKey, "meta_key");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.UmetaId).HasColumnName("umeta_id");
            entity.Property(e => e.MetaKey).HasColumnName("meta_key");
            entity.Property(e => e.MetaValue).HasColumnName("meta_value");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
