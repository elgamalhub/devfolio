using Microsoft.EntityFrameworkCore;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;
using Portfolio.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Infrastructure.Repositories
{
    public class BlogRepository : GenericRepository<BlogPost>, IBlogRepository
    {
        public BlogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<BlogPost>> GetAllAsync()
        {
            return await _dbSet
                .Include(b => b.Tags)
                .OrderByDescending(b => b.PublishedAt)
                .ToListAsync();
        }

        public override async Task<BlogPost?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(b => b.Tags)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<BlogPost?> GetBySlugAsync(string slug)
        {
            return await _dbSet
                .Include(b => b.Tags)
                .FirstOrDefaultAsync(b => b.Slug == slug && b.IsPublished);
        }

        public async Task<bool> SlugExistsAsync(string slug, int? excludePostId = null)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return false;
            }

            return await _dbSet.AnyAsync(b =>
                b.Slug == slug && (!excludePostId.HasValue || b.Id != excludePostId.Value));
        }

        public async Task<IEnumerable<BlogPost>> GetPublishedPostsAsync()
        {
            return await _dbSet
                .Include(b => b.Tags)
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.PublishedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetRecentPostsAsync(int count)
        {
            return await _dbSet
                .Include(b => b.Tags)
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.PublishedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task IncrementViewCountAsync(int id)
        {
            var post = await _dbSet.FindAsync(id);
            if (post != null)
            {
                post.ViewCount++;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<BlogPost>> SearchPostsAsync(string? searchTerm, string? tag = null)
        {
            var query = _dbSet
                .Include(b => b.Tags)
                .Where(b => b.IsPublished);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(b =>
                    b.TitleEn.Contains(searchTerm) ||
                    b.TitleAr.Contains(searchTerm) ||
                    b.ContentEn.Contains(searchTerm) ||
                    b.ContentAr.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                query = query.Where(b => b.Tags.Any(t => t.TagName == tag));
            }

            return await query
                .OrderByDescending(b => b.PublishedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetAllTagsAsync()
        {
            return await _context.BlogTags
                .Select(t => t.TagName)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }
    }
}
