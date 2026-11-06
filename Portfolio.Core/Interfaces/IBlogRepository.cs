using Portfolio.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Core.Interfaces
{
    public interface IBlogRepository : IRepository<BlogPost>
    {
        Task<IEnumerable<BlogPost>> GetPublishedPostsAsync();
        Task<BlogPost?> GetBySlugAsync(string slug);
        Task<IEnumerable<BlogPost>> GetRecentPostsAsync(int count);
        Task IncrementViewCountAsync(int id);
        Task<IEnumerable<BlogPost>> SearchPostsAsync(string searchTerm, string tag = null);
        Task<IEnumerable<string>> GetAllTagsAsync();
    }
}
