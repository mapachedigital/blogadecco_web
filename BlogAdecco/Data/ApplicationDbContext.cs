// Copyright (c) 2025, Mapache Digital
// Version: 1
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogAdecco.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<BlogAdecco.Models.ApplicationUser> ApplicationUser { get; set; }
        public DbSet<BlogAdecco.Models.Attachment> Attachment { get; set; }
        public DbSet<BlogAdecco.Models.Tag> Tag { get; set; }
        public DbSet<BlogAdecco.Models.Category> Category { get; set; }
        public DbSet<BlogAdecco.Models.Post> Post { get; set; }
    }
}
