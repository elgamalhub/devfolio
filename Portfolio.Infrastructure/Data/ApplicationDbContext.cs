using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Portfolio.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<About> Abouts { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Experience> Experiences { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTechnology> ProjectTechnologies { get; set; }
        public DbSet<SocialLink> SocialLinks { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<TypingTitle> TypingTitles { get; set; }
        public DbSet<SiteSettings> SiteSettings { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogTag> BlogTags { get; set; }
        public DbSet<Testimonial> Testimonials { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<PageView> PageViews { get; set; }
        public DbSet<ProjectClick> ProjectClicks { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<ProjectCategory> ProjectCategories { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Project - ProjectTechnology relationship
            builder.Entity<Project>()
                .HasMany(p => p.Technologies)
                .WithOne(pt => pt.Project)
                .HasForeignKey(pt => pt.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // BlogPost - BlogTag relationship
            builder.Entity<BlogPost>()
                .HasMany(b => b.Tags)
                .WithOne(t => t.BlogPost)
                .HasForeignKey(t => t.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProjectClick - Project relationship
            builder.Entity<ProjectClick>()
                .HasOne(pc => pc.Project)
                .WithMany()
                .HasForeignKey(pc => pc.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            builder.Entity<Project>()
                .HasIndex(p => p.Category);

            builder.Entity<BlogPost>()
                .HasIndex(b => b.Slug)
                .IsUnique();

            builder.Entity<PageView>()
                .HasIndex(pv => pv.ViewedAt);
        }
    }
}
