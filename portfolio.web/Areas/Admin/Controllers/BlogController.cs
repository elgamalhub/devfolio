using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;
using portfolio.web.Services;

namespace portfolio.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BlogController : Controller
    {
        private readonly IBlogRepository _blogRepo;
        private readonly IFileStorage _fileStorage;

        public BlogController(IBlogRepository blogRepo, IFileStorage fileStorage)
        {
            _blogRepo = blogRepo;
            _fileStorage = fileStorage;
        }

        public async Task<IActionResult> Index(string? search = null, string? tag = null)
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
        public async Task<IActionResult> Create(BlogPost model, IFormFile? featuredImage, string? tags)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (featuredImage != null)
                    {
                        model.FeaturedImage = await _fileStorage.SaveAsync(
                            featuredImage, "images/blog", FileUploadKind.Image);
                    }

                    model.Slug = await GenerateUniqueSlugAsync(model.TitleEn, model.TitleAr);
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
                catch (FileValidationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
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
        public async Task<IActionResult> Edit(int id, BlogPost model, IFormFile? featuredImage, string? tags)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var post = await _blogRepo.GetByIdAsync(id);
                    if (post == null)
                        return NotFound();

                    post.TitleEn = model.TitleEn;
                    post.TitleAr = model.TitleAr;
                    post.ContentEn = model.ContentEn;
                    post.ContentAr = model.ContentAr;
                    post.IsPublished = model.IsPublished;
                    post.Slug = await GenerateUniqueSlugAsync(model.TitleEn, model.TitleAr, post.Id);

                    if (featuredImage != null)
                    {
                        var newImage = await _fileStorage.SaveAsync(
                            featuredImage, "images/blog", FileUploadKind.Image);
                        _fileStorage.Delete(post.FeaturedImage);
                        post.FeaturedImage = newImage;
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
                catch (FileValidationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            ViewBag.Tags = tags;
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
                _fileStorage.Delete(post.FeaturedImage);
                await _blogRepo.DeleteAsync(id);
                TempData["Success"] = "Blog post deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> GenerateUniqueSlugAsync(string titleEn, string titleAr, int? excludePostId = null)
        {
            var baseSlug = Slugify(titleEn);
            if (string.IsNullOrWhiteSpace(baseSlug))
            {
                baseSlug = Slugify(titleAr);
            }
            if (string.IsNullOrWhiteSpace(baseSlug))
            {
                baseSlug = "post";
            }

            var slug = baseSlug;
            var suffix = 2;

            while (await _blogRepo.SlugExistsAsync(slug, excludePostId))
            {
                slug = $"{baseSlug}-{suffix}";
                suffix++;
            }

            return slug;
        }

        private static string Slugify(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);
            var previousWasSeparator = false;

            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToLowerInvariant(character));
                    previousWasSeparator = false;
                }
                else if (!previousWasSeparator)
                {
                    builder.Append('-');
                    previousWasSeparator = true;
                }
            }

            return builder.ToString().Trim('-');
        }
    }
}
