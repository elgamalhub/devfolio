using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;

namespace portfolio.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BlogController : Controller
    {
        private readonly IBlogRepository _blogRepo;
        private readonly IWebHostEnvironment _env;

        public BlogController(IBlogRepository blogRepo, IWebHostEnvironment env)
        {
            _blogRepo = blogRepo;
            _env = env;
        }

        public async Task<IActionResult> Index(string search = null, string tag = null)
        {
            IEnumerable<BlogPost> posts;

            if (!string.IsNullOrWhiteSpace(search) || !string.IsNullOrWhiteSpace(tag))
            {
                posts = await _blogRepo.SearchPostsAsync(search, tag);
            }
            else
            {
                posts = await _blogRepo.GetAllAsync();
            }

            ViewBag.AllTags = await _blogRepo.GetAllTagsAsync();
            ViewBag.Search = search;
            ViewBag.Tag = tag;

            return View(posts);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogPost model, IFormFile? featuredImage, string tags)
        {
            if (ModelState.IsValid)
            {
                if (featuredImage != null)
                    model.FeaturedImage = await SaveImageAsync(featuredImage);

                model.Slug = GenerateSlug(model.TitleEn);
                model.PublishedAt = DateTime.UtcNow;

                // Add tags
                if (!string.IsNullOrWhiteSpace(tags))
                {
                    var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tag in tagList)
                    {
                        model.Tags.Add(new BlogTag { TagName = tag.Trim() });
                    }
                }

                await _blogRepo.AddAsync(model);
                TempData["Success"] = "Blog post created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var post = await _blogRepo.GetByIdAsync(id);
            if (post == null)
                return NotFound();

            ViewBag.Tags = string.Join(", ", post.Tags.Select(t => t.TagName));
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BlogPost model, IFormFile? featuredImage, string tags)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var post = await _blogRepo.GetByIdAsync(id);
                if (post == null)
                    return NotFound();

                post.TitleEn = model.TitleEn;
                post.TitleAr = model.TitleAr;
                post.ContentEn = model.ContentEn;
                post.ContentAr = model.ContentAr;
                post.IsPublished = model.IsPublished;
                post.Slug = GenerateSlug(model.TitleEn);

                if (featuredImage != null)
                {
                    DeleteImage(post.FeaturedImage);
                    post.FeaturedImage = await SaveImageAsync(featuredImage);
                }

                // Update tags
                post.Tags.Clear();
                if (!string.IsNullOrWhiteSpace(tags))
                {
                    var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tag in tagList)
                    {
                        post.Tags.Add(new BlogTag { TagName = tag.Trim(), BlogPostId = post.Id });
                    }
                }

                await _blogRepo.UpdateAsync(post);
                TempData["Success"] = "Blog post updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
        public async Task<IActionResult> Details(int id)
        {
            var post = await _blogRepo.GetByIdAsync(id);
            if (post == null)
                return NotFound();
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _blogRepo.GetByIdAsync(id);
            if (post != null)
            {
                DeleteImage(post.FeaturedImage);
                await _blogRepo.DeleteAsync(id);
                TempData["Success"] = "Blog post deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private string GenerateSlug(string title)
        {
            return title.ToLower()
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("'", "")
                .Replace("\"", "");
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "blog");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/images/blog/" + uniqueFileName;
        }

        private void DeleteImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            var fullPath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}