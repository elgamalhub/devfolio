using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;

namespace portfolio.web.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogRepository _blogRepo;

        public BlogController(IBlogRepository blogRepo)
        {
            _blogRepo = blogRepo;
        }

        public async Task<IActionResult> Index(string lang = "en", string search = null, string tag = null)
        {
            Response.Cookies.Append("language", lang, new CookieOptions { Expires = DateTimeOffset.Now.AddYears(1) });

            IEnumerable<BlogPost> posts;

            if (!string.IsNullOrWhiteSpace(search) || !string.IsNullOrWhiteSpace(tag))
            {
                posts = await _blogRepo.SearchPostsAsync(search, tag);
            }
            else
            {
                posts = await _blogRepo.GetPublishedPostsAsync();
            }

            ViewBag.Language = lang;
            ViewBag.AllTags = await _blogRepo.GetAllTagsAsync();
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentTag = tag;

            return View(posts);
        }

        public async Task<IActionResult> Post(string slug, string lang = "en")
        {
            // Set cookie for language preference
            Response.Cookies.Append("language", lang, new CookieOptions { Expires = DateTimeOffset.Now.AddYears(1) });

            var post = await _blogRepo.GetBySlugAsync(slug);

            if (post == null)
                return NotFound();

            await _blogRepo.IncrementViewCountAsync(post.Id);
            ViewBag.Language = lang;

            return View(post);
        }
    }
}