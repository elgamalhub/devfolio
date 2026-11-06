using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.DTOs;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;
using portfolio.web.Services;

namespace portfolio.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProjectsController : Controller
    {
        private const string DefaultProjectImage = "/images/projects/default-project.jpg";

        private readonly IProjectRepository _projectRepo;
        private readonly IFileStorage _fileStorage;

        public ProjectsController(IProjectRepository projectRepo, IFileStorage fileStorage)
        {
            _projectRepo = projectRepo;
            _fileStorage = fileStorage;
        }

        public async Task<IActionResult> Index()
        {
            var projects = await _projectRepo.GetAllAsync();
            return View(projects);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var imagePath = DefaultProjectImage;
                    if (model.Image != null && model.Image.Length > 0)
                    {
                        imagePath = await _fileStorage.SaveAsync(
                            model.Image, "images/projects", FileUploadKind.Image);
                    }

                    var project = new Project
                    {
                        TitleEn = model.TitleEn,
                        TitleAr = model.TitleAr,
                        ShortDescriptionEn = model.ShortDescriptionEn,
                        ShortDescriptionAr = model.ShortDescriptionAr,
                        FullDescriptionEn = model.FullDescriptionEn ?? "",
                        FullDescriptionAr = model.FullDescriptionAr ?? "",
                        ImagePath = imagePath,
                        IconClass = model.IconClass ?? "fas fa-code",
                        DemoUrl = model.DemoUrl ?? "",
                        GithubUrl = model.GithubUrl ?? "",
                        Category = model.Category,
                        IsHidden = model.IsHidden,
                        Order = model.Order,
                        IsActive = model.IsActive,
                        CreatedAt = DateTime.UtcNow,
                        Technologies = new List<ProjectTechnology>()
                    };

                    // Add technologies
                    if (!string.IsNullOrWhiteSpace(model.Technologies))
                    {
                        var techList = model.Technologies.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var tech in techList)
                        {
                            project.Technologies.Add(new ProjectTechnology
                            {
                                TechnologyName = tech.Trim()
                            });
                        }
                    }

                    await _projectRepo.AddAsync(project);
                    TempData["Success"] = "Project created successfully!";
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
            var project = await _projectRepo.GetByIdAsync(id);
            if (project == null)
                return NotFound();

            var model = new ProjectViewModel
            {
                Id = project.Id,
                TitleEn = project.TitleEn,
                TitleAr = project.TitleAr,
                ShortDescriptionEn = project.ShortDescriptionEn,
                ShortDescriptionAr = project.ShortDescriptionAr,
                FullDescriptionEn = project.FullDescriptionEn,
                FullDescriptionAr = project.FullDescriptionAr,
                ExistingImagePath = project.ImagePath,
                IconClass = project.IconClass,
                DemoUrl = project.DemoUrl,
                GithubUrl = project.GithubUrl,
                Category = project.Category,
                IsHidden = project.IsHidden,
                Order = project.Order,
                IsActive = project.IsActive,
                Technologies = string.Join(", ", project.Technologies.Select(t => t.TechnologyName))
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var project = await _projectRepo.GetByIdAsync(id);
                    if (project == null)
                        return NotFound();

                    project.TitleEn = model.TitleEn;
                    project.TitleAr = model.TitleAr;
                    project.ShortDescriptionEn = model.ShortDescriptionEn;
                    project.ShortDescriptionAr = model.ShortDescriptionAr;
                    project.FullDescriptionEn = model.FullDescriptionEn;
                    project.FullDescriptionAr = model.FullDescriptionAr;
                    project.IconClass = model.IconClass;
                    project.DemoUrl = model.DemoUrl;
                    project.GithubUrl = model.GithubUrl;
                    project.Category = model.Category;
                    project.IsHidden = model.IsHidden;
                    project.Order = model.Order;
                    project.IsActive = model.IsActive;

                    // Save the replacement first so a rejected upload never removes the current image.
                    if (model.Image != null)
                    {
                        var newImage = await _fileStorage.SaveAsync(
                            model.Image, "images/projects", FileUploadKind.Image);
                        _fileStorage.Delete(project.ImagePath);
                        project.ImagePath = newImage;
                    }

                    // Update technologies
                    project.Technologies.Clear();
                    if (!string.IsNullOrWhiteSpace(model.Technologies))
                    {
                        var techList = model.Technologies.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var tech in techList)
                        {
                            project.Technologies.Add(new ProjectTechnology
                            {
                                TechnologyName = tech.Trim(),
                                ProjectId = project.Id
                            });
                        }
                    }

                    await _projectRepo.UpdateAsync(project);
                    TempData["Success"] = "Project updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (FileValidationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var project = await _projectRepo.GetByIdAsync(id);
            if (project == null)
                return NotFound();

            return View(project);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var project = await _projectRepo.GetByIdAsync(id);
            if (project == null)
                return NotFound();

            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _projectRepo.GetByIdAsync(id);
            if (project != null)
            {
                _fileStorage.Delete(project.ImagePath);
                await _projectRepo.DeleteAsync(id);
                TempData["Success"] = "Project deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}