using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Core.DTOs
{
    public class DashboardViewModel
    {
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int UnreadMessages { get; set; }
        public int TotalPageViews { get; set; }
        public int TodayPageViews { get; set; }
        public int TotalBlogPosts { get; set; }
        public IEnumerable<ProjectClickStats> TopProjects { get; set; } = new List<ProjectClickStats>();
        public IEnumerable<RecentMessage> RecentMessages { get; set; } = new List<RecentMessage>();
    }

    public class ProjectClickStats
    {
        public string ProjectTitle { get; set; } = string.Empty;
        public int ClickCount { get; set; }
    }

    public class RecentMessage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
