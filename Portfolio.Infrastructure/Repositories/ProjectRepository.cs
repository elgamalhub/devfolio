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
    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        public ProjectRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.Technologies)
                .OrderBy(p => p.Order)
                .ToListAsync();
        }

        public override async Task<Project?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Technologies)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Project>> GetVisibleProjectsAsync()
        {
            return await _dbSet
                .Include(p => p.Technologies)
                .Where(p => p.IsActive && !p.IsHidden)
                .OrderBy(p => p.Order)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetProjectsByCategoryAsync(string category)
        {
            return await _dbSet
                .Include(p => p.Technologies)
                .Where(p => p.IsActive && p.Category == category)
                .OrderBy(p => p.Order)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetHiddenProjectsAsync()
        {
            return await _dbSet
                .Include(p => p.Technologies)
                .Where(p => p.IsActive && p.IsHidden)
                .OrderBy(p => p.Order)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetActiveProjectsOrderedAsync()
        {
            return await _dbSet
                .Include(p => p.Technologies)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Order)
                .ToListAsync();
        }
    }
}
