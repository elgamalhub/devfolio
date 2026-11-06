using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;

namespace portfolio.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProjectCategoriesController : Controller
    {
        private readonly IRepository<ProjectCategory> _categoryRepo;

        public ProjectCategoriesController(IRepository<ProjectCategory> categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<IActionResult> Index()
        {
            var categories = (await _categoryRepo.GetAllAsync()).OrderBy(c => c.Order);
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCategory model)
        {
            if (ModelState.IsValid)
            {
                // Generate slug from English name
                model.Slug = model.NameEn.ToLower()
                    .Replace(" ", "-")
                    .Replace(".", "")
                    .Replace(",", "");

                await _categoryRepo.AddAsync(model);
                TempData["Success"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectCategory model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                // Update slug
                model.Slug = model.NameEn.ToLower()
                    .Replace(" ", "-")
                    .Replace(".", "")
                    .Replace(",", "");

                await _categoryRepo.UpdateAsync(model);
                TempData["Success"] = "Category updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoryRepo.DeleteAsync(id);
            TempData["Success"] = "Category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category != null)
            {
                category.IsActive = !category.IsActive;
                await _categoryRepo.UpdateAsync(category);
                TempData["Success"] = "Category status updated!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}