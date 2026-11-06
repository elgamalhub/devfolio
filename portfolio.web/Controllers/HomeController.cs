using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Portfolio.Core.DTOs;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;
using Portfolio.Infrastructure.Data;

namespace Portfolio.Web.Controllers
{
    public class HomeController : Controller
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            Encoder = JavaScriptEncoder.Default
        };

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
            var (currentLang, currentTheme, settings) = await LoadPublicPreferencesAsync(lang, theme);

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

            ViewBag.Sections = visibleSections;
            ViewBag.CurrentTheme = currentTheme;
            ViewBag.IsHomePage = true;
            ViewBag.SocialLinks = viewModel.SocialLinks;
            ViewBag.HasCertificates = viewModel.Certificates.Any();
            ViewBag.TypingTitles = viewModel.TypingTitles
                .Select(t => currentLang == "ar" ? t.TitleAr : t.TitleEn)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            ViewData["CanonicalUrl"] = Url.Action("Index", "Home", new { lang = currentLang }, Request.Scheme);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var name = currentLang == "ar" ? viewModel.About?.NameAr : viewModel.About?.NameEn;
            var jobTitle = currentLang == "ar"
                ? viewModel.TypingTitles.FirstOrDefault()?.TitleAr
                : viewModel.TypingTitles.FirstOrDefault()?.TitleEn;

            ViewData["StructuredData"] = JsonSerializer.Serialize(new
            {
                @context = "https://schema.org",
                @type = "Person",
                name,
                url = baseUrl,
                image = ToAbsoluteUrl(viewModel.About?.ProfileImage, baseUrl),
                jobTitle,
                email = viewModel.About?.Email,
                address = new
                {
                    @type = "PostalAddress",
                    addressLocality = viewModel.About?.Location
                },
                sameAs = viewModel.SocialLinks.Select(s => s.Url).Where(u => !string.IsNullOrWhiteSpace(u)).ToArray(),
                knowsLanguage = new[] { "en", "ar" }
            }, JsonOptions);

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
            ViewBag.Language = NormalizeLanguage(Request.Cookies["language"] ?? "en");

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetTheme([FromBody] ThemePreferenceDto? model)
        {
            var theme = NormalizeTheme(model?.Theme);
            SetPreferenceCookie("theme", theme);
            return Json(new { success = true, theme });
        }

        public async Task<IActionResult> Privacy()
        {
            await LoadPublicPreferencesAsync(null, null);
            ViewData["Title"] = "Privacy Policy";
            ViewData["Description"] = "How this portfolio handles contact messages, cookies, and basic analytics.";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<(string Language, string Theme, Dictionary<string, string> Settings)> LoadPublicPreferencesAsync(
            string? lang,
            string? theme)
        {
            var settingEntities = (await _settingsRepo.GetAllAsync()).ToList();
            var defaults = settingEntities.ToDictionary(
                s => s.Key,
                s => s.ValueEn,
                StringComparer.OrdinalIgnoreCase);

            // Priority: explicit query parameter > cookie > site default.
            var currentLang = NormalizeLanguage(
                lang ?? Request.Cookies["language"] ?? defaults.GetValueOrDefault("default_language", "en"));

            var currentTheme = NormalizeTheme(
                theme ?? Request.Cookies["theme"] ?? defaults.GetValueOrDefault("default_theme", "dark"));

            // Persist explicit preferences only.
            if (lang != null)
            {
                SetPreferenceCookie("language", currentLang);
            }

            if (theme != null)
            {
                SetPreferenceCookie("theme", currentTheme);
            }

            var settings = settingEntities.ToDictionary(
                s => s.Key,
                s => currentLang == "ar" && !string.IsNullOrWhiteSpace(s.ValueAr) ? s.ValueAr : s.ValueEn,
                StringComparer.OrdinalIgnoreCase);

            ViewBag.Settings = settings;
            ViewBag.Language = currentLang;
            ViewBag.CurrentLanguage = currentLang;
            ViewBag.CurrentTheme = currentTheme;

            return (currentLang, currentTheme, settings);
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

        private static string ToAbsoluteUrl(string? path, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            return path.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? path
                : $"{baseUrl}/{path.TrimStart('/')}";
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

    public class ThemePreferenceDto
    {
        public string? Theme { get; set; }
    }
}
