using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;

namespace portfolio.web.Controllers
{
    public class BlogController : Controller
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            Encoder = JavaScriptEncoder.Default
        };

        private readonly IBlogRepository _blogRepo;
        private readonly IRepository<SiteSettings> _settingsRepo;
        private readonly IRepository<SocialLink> _socialRepo;

        public BlogController(
            IBlogRepository blogRepo,
            IRepository<SiteSettings> settingsRepo,
            IRepository<SocialLink> socialRepo)
        {
            _blogRepo = blogRepo;
            _settingsRepo = settingsRepo;
            _socialRepo = socialRepo;
        }

        public async Task<IActionResult> Index(string? lang = null, string? search = null, string? tag = null, string? theme = null)
        {
            var currentLang = await LoadPublicContextAsync(lang, theme);
            var isFiltered = !string.IsNullOrWhiteSpace(search) || !string.IsNullOrWhiteSpace(tag);

            IEnumerable<BlogPost> posts;

            if (isFiltered)
            {
                posts = await _blogRepo.SearchPostsAsync(search, tag);
            }
            else
            {
                posts = await _blogRepo.GetPublishedPostsAsync();
            }

            var postList = posts.ToList();

            ViewBag.Language = currentLang;
            ViewBag.AllTags = await _blogRepo.GetAllTagsAsync();
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentTag = tag;
            ViewBag.ReadingTimes = postList.ToDictionary(
                p => p.Id,
                p => GetReadingTime(currentLang == "ar" ? p.ContentAr : p.ContentEn));

            var pageTitle = currentLang == "ar" ? "المدونة" : "Blog";
            var description = currentLang == "ar"
                ? "مقالات وملاحظات عن تطوير .NET وهندسة البرمجيات."
                : "Articles and notes on .NET development, architecture, and software engineering.";

            ViewData["Title"] = pageTitle;
            ViewData["Description"] = description;
            ViewData["NoIndex"] = isFiltered;
            ViewData["CanonicalUrl"] = Url.Action("Index", "Blog", new { lang = currentLang }, Request.Scheme);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            ViewData["StructuredData"] = JsonSerializer.Serialize(new
            {
                @context = "https://schema.org",
                @type = "Blog",
                name = $"{pageTitle} — {GetSiteTitle(currentLang)}",
                url = Url.Action("Index", "Blog", new { lang = currentLang }, Request.Scheme),
                description,
                blogPost = postList.Take(12).Select(p => new
                {
                    @type = "BlogPosting",
                    headline = currentLang == "ar" ? p.TitleAr : p.TitleEn,
                    url = Url.Action("Post", "Blog", new { slug = p.Slug, lang = currentLang }, Request.Scheme),
                    datePublished = p.PublishedAt.ToString("o"),
                    image = ToAbsoluteUrl(p.FeaturedImage, baseUrl)
                })
            }, JsonOptions);

            return View(postList);
        }

        [HttpGet("Blog/Post/{slug}")]
        public async Task<IActionResult> Post(string slug, string? lang = null, string? theme = null)
        {
            var post = await _blogRepo.GetBySlugAsync(slug);

            if (post == null)
            {
                return NotFound();
            }

            await _blogRepo.IncrementViewCountAsync(post.Id);

            var currentLang = await LoadPublicContextAsync(lang, theme);
            var title = currentLang == "ar" ? post.TitleAr : post.TitleEn;
            var content = currentLang == "ar" ? post.ContentAr : post.ContentEn;
            var description = BuildExcerpt(content, 160);

            ViewBag.Language = currentLang;
            ViewBag.ReadingTime = GetReadingTime(content);

            var allPosts = (await _blogRepo.GetPublishedPostsAsync()).ToList();
            var relatedPosts = allPosts
                .Where(p => p.Id != post.Id)
                .OrderByDescending(p => p.Tags.Count(t => post.Tags.Any(pt => pt.TagName == t.TagName)))
                .ThenByDescending(p => p.PublishedAt)
                .Take(3)
                .ToList();

            ViewBag.RelatedPosts = relatedPosts;

            ViewData["Title"] = title;
            ViewData["Description"] = description;
            ViewData["OgType"] = "article";
            ViewData["OgImage"] = post.FeaturedImage;
            ViewData["CanonicalUrl"] = Url.Action("Post", "Blog", new { slug = post.Slug, lang = currentLang }, Request.Scheme);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            ViewData["StructuredData"] = JsonSerializer.Serialize(new
            {
                @context = "https://schema.org",
                @type = "BlogPosting",
                headline = title,
                description,
                datePublished = post.PublishedAt.ToString("o"),
                dateModified = post.PublishedAt.ToString("o"),
                image = ToAbsoluteUrl(post.FeaturedImage, baseUrl),
                url = Url.Action("Post", "Blog", new { slug = post.Slug, lang = currentLang }, Request.Scheme),
                mainEntityOfPage = new
                {
                    @type = "WebPage",
                    @id = Url.Action("Post", "Blog", new { slug = post.Slug, lang = currentLang }, Request.Scheme)
                },
                author = new
                {
                    @type = "Person",
                    name = currentLang == "ar" ? "أحمد عبد الرحمن الجمل" : "Ahmed Abdelrahman Elgamal",
                    url = Url.Action("Index", "Home", null, Request.Scheme)
                },
                publisher = new
                {
                    @type = "Organization",
                    name = GetSiteTitle(currentLang),
                    url = baseUrl
                },
                keywords = string.Join(", ", post.Tags.Select(t => t.TagName)),
                inLanguage = currentLang
            }, JsonOptions);

            return View(post);
        }

        private async Task<string> LoadPublicContextAsync(string? lang, string? theme)
        {
            var settingEntities = (await _settingsRepo.GetAllAsync()).ToList();
            var defaults = settingEntities.ToDictionary(
                s => s.Key,
                s => s.ValueEn,
                StringComparer.OrdinalIgnoreCase);

            var currentLang = NormalizeLanguage(
                lang ?? Request.Cookies["language"] ?? defaults.GetValueOrDefault("default_language", "en"));

            var currentTheme = NormalizeTheme(
                theme ?? Request.Cookies["theme"] ?? defaults.GetValueOrDefault("default_theme", "dark"));

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
            ViewBag.SocialLinks = (await _socialRepo.GetAllAsync())
                .Where(s => s.IsActive)
                .OrderBy(s => s.Order)
                .ToList();

            return currentLang;
        }

        private string GetSiteTitle(string lang)
        {
            var settings = ViewBag.Settings as Dictionary<string, string>;
            return lang == "ar"
                ? settings?.GetValueOrDefault("site_title_ar", "أحمد الجمل") ?? "أحمد الجمل"
                : settings?.GetValueOrDefault("site_title_en", "Ahmed Elgamal") ?? "Ahmed Elgamal";
        }

        private static int GetReadingTime(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return 1;
            }

            var text = Regex.Replace(html, "<.*?>", " ");
            var words = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
            return Math.Max(1, (int)Math.Ceiling(words / 200.0));
        }

        private static string BuildExcerpt(string? html, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var text = Regex.Replace(html, "<.*?>", " ");
            text = Regex.Replace(text, @"\s+", " ").Trim();
            return text.Length > maxLength ? text[..maxLength].TrimEnd() + "…" : text;
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
    }
}
