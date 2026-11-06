using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Core.DTOs;
using Portfolio.Core.Interfaces;
using Portfolio.Infrastructure.Data;

namespace portfolio.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IProjectRepository _projectRepo;
        private readonly ApplicationDbContext _context;

        public DashboardController(IProjectRepository projectRepo, ApplicationDbContext context)
        {
            _projectRepo = projectRepo;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;

            var stats = new DashboardViewModel
            {
                TotalProjects = await _context.Projects.CountAsync(),
                ActiveProjects = await _context.Projects.CountAsync(p => p.IsActive),
                UnreadMessages = await _context.ContactMessages.CountAsync(m => !m.IsRead),
                TotalPageViews = await _context.PageViews.CountAsync(),
                TodayPageViews = await _context.PageViews.CountAsync(pv => pv.ViewedAt.Date == today),
                TotalBlogPosts = await _context.BlogPosts.CountAsync(),

                TopProjects = await _context.ProjectClicks
                    .GroupBy(pc => new { pc.ProjectId, pc.Project.TitleEn })
                    .Select(g => new ProjectClickStats
                    {
                        ProjectTitle = g.Key.TitleEn,
                        ClickCount = g.Count()
                    })
                    .OrderByDescending(p => p.ClickCount)
                    .Take(5)
                    .ToListAsync(),

                RecentMessages = await _context.ContactMessages
                    .OrderByDescending(m => m.ReceivedAt)
                    .Take(5)
                    .Select(m => new RecentMessage
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Email = m.Email,
                        ReceivedAt = m.ReceivedAt,
                        IsRead = m.IsRead
                    })
                    .ToListAsync()
            };

            return View(stats);
        }
    }
}