using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Portfolio.Core.DTOs;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;
using Portfolio.Infrastructure.Data;
using System.Diagnostics;

namespace Portfolio.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<About> _aboutRepo;
        private readonly IRepository<Skill> _skillRepo;
        private readonly IRepository<Experience> _expRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IRepository<SocialLink> _socialRepo;
        private readonly IRepository<TypingTitle> _titleRepo;
        private readonly IRepository<Testimonial> _testimonialRepo;
        private readonly IRepository<Certificate> _certificateRepo;
        private readonly IRepository<ContactMessage> _messageRepo;
        private readonly IRepository<SiteSettings> _settingsRepo;
        private readonly IRepository<Section> _sectionRepo;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IRepository<About> aboutRepo,
            IRepository<Skill> skillRepo,
            IRepository<Experience> expRepo,
            IProjectRepository projectRepo,
            IRepository<SocialLink> socialRepo,
            IRepository<TypingTitle> titleRepo,
            IRepository<Testimonial> testimonialRepo,
            IRepository<Certificate> certificateRepo,
            IRepository<ContactMessage> messageRepo,
            IRepository<SiteSettings> settingsRepo,
            IRepository<Section> sectionRepo,
            ApplicationDbContext context,
            ILogger<HomeController> logger)
        {
            _aboutRepo = aboutRepo;
            _skillRepo = skillRepo;
            _expRepo = expRepo;
            _projectRepo = projectRepo;
            _socialRepo = socialRepo;
            _titleRepo = titleRepo;
            _testimonialRepo = testimonialRepo;
            _certificateRepo = certificateRepo;
            _messageRepo = messageRepo;
            _settingsRepo = settingsRepo;
            _sectionRepo = sectionRepo;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? lang, string? theme)
        {
            // Get site settings
            var settings = (await _settingsRepo.GetAllAsync())
                .ToDictionary(s => s.Key, s => s.ValueEn, StringComparer.OrdinalIgnoreCase);

            // Determine language (priority: query param > cookie > setting > default)
            var currentLang = NormalizeLanguage(
                lang ?? Request.Cookies["language"] ?? settings.GetValueOrDefault("default_language", "en"));

            // Determine theme (priority: query param > cookie > setting > default)
            var currentTheme = NormalizeTheme(
                theme ?? Request.Cookies["theme"] ?? settings.GetValueOrDefault("default_theme", "dark"));

            // Persist explicit preferences only
            if (lang != null)
            {
                SetPreferenceCookie("language", currentLang);
            }
            if (theme != null)
            {
                SetPreferenceCookie("theme", currentTheme);
            }

            // Get project categories
            var categories = (await _context.ProjectCategories.ToListAsync())
                .Where(c => c.IsActive)
                .OrderBy(c => c.Order)
                .ToList();

            ViewBag.Categories = categories;

            // Get recent blog posts
            var recentPosts = await _context.BlogPosts
                .Include(b => b.Tags)
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.PublishedAt)
                .Take(3)
                .ToListAsync();

            ViewBag.RecentPosts = recentPosts;

            // Track page view (at most once per session, never for administrators or crawlers)
            await TrackPageView("/");

            // Get visible sections
            var visibleSections = (await _sectionRepo.GetAllAsync())
                .Where(s => s.IsVisible)
                .OrderBy(s => s.Order)
                .ToList();

            var viewModel = new HomeViewModel
            {
                About = (await _aboutRepo.GetAllAsync()).FirstOrDefault(),
                Skills = (await _skillRepo.GetAllAsync())
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Order)
                    .ToList(),
                Experiences = (await _expRepo.GetAllAsync())
                    .Where(e => e.IsActive)
                    .OrderByDescending(e => e.StartDate)
                    .ToList(),
                Projects = await _projectRepo.GetActiveProjectsOrderedAsync(),
                SocialLinks = (await _socialRepo.GetAllAsync())
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Order)
                    .ToList(),
                TypingTitles = (await _titleRepo.GetAllAsync())
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Order)
                    .ToList(),
                Testimonials = (await _testimonialRepo.GetAllAsync())
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Order)
                    .ToList(),
                Certificates = (await _certificateRepo.GetAllAsync())
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Order)
                    .ToList(),
                CurrentLanguage = currentLang
            };

            ViewBag.Settings = settings;
            ViewBag.Sections = visibleSections;
            ViewBag.CurrentTheme = currentTheme;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("contact")]
        public async Task<IActionResult> SendMessage([FromBody] ContactMessageDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(message => !string.IsNullOrWhiteSpace(message));

                return BadRequest(new { success = false, message = string.Join(" ", errors) });
            }

            try
            {
                var message = new ContactMessage
                {
                    Name = model.Name.Trim(),
                    Email = model.Email.Trim(),
                    Message = model.Message.Trim(),
                    ReceivedAt = DateTime.UtcNow,
                    IsRead = false
                };

                await _messageRepo.AddAsync(message);

                return Json(new { success = true, message = "Message sent successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending contact message.");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "An error occurred while sending the message. Please try again." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectsByCategory(string? category)
        {
            IEnumerable<Project> projects;

            if (string.IsNullOrWhiteSpace(category) || category == "all")
            {
                projects = await _projectRepo.GetActiveProjectsOrderedAsync();
            }
            else
            {
                projects = await _projectRepo.GetProjectsByCategoryAsync(category);
            }

            return PartialView("_ProjectCards", projects);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("tracking")]
        public async Task<IActionResult> TrackProjectClick([FromBody] ProjectClickDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false });
            }

            var projectExists = await _context.Projects
                .AnyAsync(p => p.Id == model.ProjectId && p.IsActive);

            if (!projectExists)
            {
                return NotFound(new { success = false });
            }

            var click = new ProjectClick
            {
                ProjectId = model.ProjectId,
                ClickedAt = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _context.ProjectClicks.Add(click);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task TrackPageView(string pageUrl)
        {
            if (User.IsInRole("Admin") || HttpContext.Session.GetString("PageViewTracked") != null)
            {
                return;
            }

            var userAgent = Request.Headers.UserAgent.ToString();
            if (IsCrawler(userAgent))
            {
                return;
            }

            HttpContext.Session.SetString("PageViewTracked", "1");

            try
            {
                var pageView = new PageView
                {
                    PageUrl = pageUrl,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = userAgent.Length > 512 ? userAgent[..512] : userAgent,
                    ViewedAt = DateTime.UtcNow
                };

                _context.PageViews.Add(pageView);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Analytics must never prevent the public page from loading.
                _logger.LogWarning(ex, "Could not record page view for {PageUrl}.", pageUrl);
            }
        }

        private void SetPreferenceCookie(string name, string value)
        {
            Response.Cookies.Append(name, value, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps,
                Path = "/"
            });
        }

        private static string NormalizeLanguage(string? language)
        {
            return string.Equals(language, "ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";
        }

        private static string NormalizeTheme(string? theme)
        {
            return string.Equals(theme, "light", StringComparison.OrdinalIgnoreCase) ? "light" : "dark";
        }

        private static bool IsCrawler(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                return false;
            }

            string[] markers =
            {
                "bot", "crawler", "spider", "slurp", "bingpreview",
                "facebookexternalhit", "headless", "lighthouse", "pingdom"
            };

            return markers.Any(marker => userAgent.Contains(marker, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
