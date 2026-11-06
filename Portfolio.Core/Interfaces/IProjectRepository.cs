using Portfolio.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Core.Interfaces
{
    public interface IProjectRepository : IRepository<Project>
    {
        Task<IEnumerable<Project>> GetVisibleProjectsAsync();
        Task<IEnumerable<Project>> GetProjectsByCategoryAsync(string category);
        Task<IEnumerable<Project>> GetHiddenProjectsAsync();
        Task<IEnumerable<Project>> GetActiveProjectsOrderedAsync();
    }
}
