using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;

namespace portfolio.web.Controllers
{
    public class SeoController : Controller
    {
        private readonly IBlogRepository _blogRepo;
        private readonly IRepository<SiteSettings> _settingsRepo;

        public SeoController(IBlogRepository blogRepo, IRepository<SiteSettings> settingsRepo)
        {
            _blogRepo = blogRepo;
            _settingsRepo = settingsRepo;
        }

        [HttpGet("/sitemap.xml")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Sitemap()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var posts = (await _blogRepo.GetPublishedPostsAsync()).ToList();

            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            AppendUrl(xml, $"{baseUrl}/?lang=en", null, "weekly", "1.0");
            AppendUrl(xml, $"{baseUrl}/?lang=ar", null, "weekly", "1.0");
            AppendUrl(xml, $"{baseUrl}/Blog?lang=en", null, "weekly", "0.9");
            AppendUrl(xml, $"{baseUrl}/Blog?lang=ar", null, "weekly", "0.9");
            AppendUrl(xml, $"{baseUrl}/Home/Privacy", null, "yearly", "0.3");

            foreach (var post in posts)
            {
                AppendUrl(
                    xml,
                    $"{baseUrl}/Blog/Post/{Uri.EscapeDataString(post.Slug)}?lang=en",
                    post.PublishedAt,
                    "monthly",
                    "0.8");

                AppendUrl(
                    xml,
                    $"{baseUrl}/Blog/Post/{Uri.EscapeDataString(post.Slug)}?lang=ar",
                    post.PublishedAt,
                    "monthly",
                    "0.8");
            }

            xml.AppendLine("</urlset>");

            return Content(xml.ToString(), "application/xml; charset=utf-8");
        }

        [HttpGet("/robots.txt")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public IActionResult Robots()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var content = new StringBuilder()
                .AppendLine("User-agent: *")
                .AppendLine("Allow: /")
                .AppendLine("Disallow: /Admin")
                .AppendLine("Disallow: /Identity")
                .AppendLine("Disallow: /Home/SendMessage")
                .AppendLine("Disallow: /Home/TrackProjectClick")
                .AppendLine()
                .AppendLine($"Sitemap: {baseUrl}/sitemap.xml")
                .ToString();

            return Content(content, "text/plain; charset=utf-8");
        }

        [HttpGet("/feed.xml")]
        [ResponseCache(Duration = 1800, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Feed()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var settings = (await _settingsRepo.GetAllAsync()).ToList();
            var siteTitle = settings.FirstOrDefault(s => s.Key == "site_title_en")?.ValueEn ?? "Ahmed Elgamal";
            var siteDescription = settings.FirstOrDefault(s => s.Key == "meta_description_en")?.ValueEn
                ?? "Notes on .NET, architecture, and software engineering.";
            var posts = (await _blogRepo.GetPublishedPostsAsync()).Take(20).ToList();

            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<rss version=\"2.0\" xmlns:atom=\"http://www.w3.org/2005/Atom\">");
            xml.AppendLine("<channel>");
            xml.AppendLine($"<title>{Escape(siteTitle)} — Blog</title>");
            xml.AppendLine($"<link>{Escape($"{baseUrl}/Blog")}</link>");
            xml.AppendLine($"<description>{Escape(siteDescription)}</description>");
            xml.AppendLine("<language>en</language>");
            xml.AppendLine($"<atom:link href=\"{Escape($"{baseUrl}/feed.xml")}\" rel=\"self\" type=\"application/rss+xml\" />");

            foreach (var post in posts)
            {
                var postUrl = $"{baseUrl}/Blog/Post/{Uri.EscapeDataString(post.Slug)}?lang=en";
                var excerpt = BuildExcerpt(post.ContentEn, 300);

                xml.AppendLine("<item>");
                xml.AppendLine($"<title>{Escape(post.TitleEn)}</title>");
                xml.AppendLine($"<link>{Escape(postUrl)}</link>");
                xml.AppendLine($"<guid isPermaLink=\"true\">{Escape(postUrl)}</guid>");
                xml.AppendLine($"<pubDate>{post.PublishedAt.ToString("R")}</pubDate>");
                xml.AppendLine($"<description>{Escape(excerpt)}</description>");
                foreach (var tag in post.Tags)
                {
                    xml.AppendLine($"<category>{Escape(tag.TagName)}</category>");
                }
                xml.AppendLine("</item>");
            }

            xml.AppendLine("</channel>");
            xml.AppendLine("</rss>");

            return Content(xml.ToString(), "application/rss+xml; charset=utf-8");
        }

        private static void AppendUrl(StringBuilder xml, string url, DateTime? lastModified, string changeFrequency, string priority)
        {
            xml.AppendLine("<url>");
            xml.AppendLine($"<loc>{Escape(url)}</loc>");
            if (lastModified.HasValue)
            {
                xml.AppendLine($"<lastmod>{lastModified.Value:yyyy-MM-dd}</lastmod>");
            }
            xml.AppendLine($"<changefreq>{changeFrequency}</changefreq>");
            xml.AppendLine($"<priority>{priority}</priority>");
            xml.AppendLine("</url>");
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

        private static string Escape(string value)
        {
            return SecurityElement.Escape(value) ?? string.Empty;
        }
    }
}
