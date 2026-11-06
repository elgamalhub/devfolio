using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;

namespace Portfolio.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SiteSettingsController : Controller
    {
        private readonly IRepository<SiteSettings> _settingsRepo;
        private readonly IRepository<Section> _sectionRepo;
        private readonly IWebHostEnvironment _env;

        public SiteSettingsController(IRepository<SiteSettings> settingsRepo, IRepository<Section> sectionRepo, IWebHostEnvironment env)
        {
            _settingsRepo = settingsRepo;
            _sectionRepo = sectionRepo;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _settingsRepo.GetAllAsync();
            ViewBag.Sections = (await _sectionRepo.GetAllAsync()).OrderBy(s => s.Order);
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSettings(Dictionary<string, string> settings)
        {
            var allSettings = await _settingsRepo.GetAllAsync();

            foreach (var setting in allSettings)
            {
                if (settings.ContainsKey(setting.Key))
                {
                    setting.ValueEn = settings[setting.Key];
                    setting.ValueAr = settings[setting.Key];
                    await _settingsRepo.UpdateAsync(setting);
                }
            }

            TempData["Success"] = "Settings updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Section Management
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSection(Section model)
        {
            ModelState.Remove("CustomContent");

            if (ModelState.IsValid)
            {
                var maxOrder = (await _sectionRepo.GetAllAsync()).Any()
                    ? (await _sectionRepo.GetAllAsync()).Max(s => s.Order)
                    : 0;

                model.Order = maxOrder + 1;
                model.IsBuiltIn = false;
                model.CreatedAt = DateTime.UtcNow;

                await _sectionRepo.AddAsync(model);
                TempData["Success"] = "Section added successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSection(int id, string nameEn, string nameAr, string iconClass, bool isVisible, bool showInNavbar, int order)
        {
            var section = await _sectionRepo.GetByIdAsync(id);
            if (section != null)
            {
                section.NameEn = nameEn;
                section.NameAr = nameAr;
                section.IconClass = iconClass;
                section.IsVisible = isVisible;
                section.ShowInNavbar = showInNavbar;
                section.Order = order;

                await _sectionRepo.UpdateAsync(section);
                TempData["Success"] = "Section updated successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSection(int id)
        {
            var section = await _sectionRepo.GetByIdAsync(id);
            if (section != null && !section.IsBuiltIn)
            {
                await _sectionRepo.DeleteAsync(id);
                TempData["Success"] = "Section deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Cannot delete built-in section!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSectionVisibility(int id)
        {
            var section = await _sectionRepo.GetByIdAsync(id);
            if (section != null)
            {
                section.IsVisible = !section.IsVisible;
                await _sectionRepo.UpdateAsync(section);
                TempData["Success"] = "Section visibility updated!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderSections(int[] sectionIds)
        {
            for (int i = 0; i < sectionIds.Length; i++)
            {
                var section = await _sectionRepo.GetByIdAsync(sectionIds[i]);
                if (section != null)
                {
                    section.Order = i + 1;
                    await _sectionRepo.UpdateAsync(section);
                }
            }

            return Json(new { success = true });
        }
        public async Task<IActionResult> EditCustomSection(int id)
        {
            var section = await _sectionRepo.GetByIdAsync(id);
            if (section == null || section.IsBuiltIn)
                return NotFound();

            return View(section);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomSection(int id, string customContent)
        {
            var section = await _sectionRepo.GetByIdAsync(id);
            if (section != null && !section.IsBuiltIn)
            {
                section.CustomContent = customContent;
                await _sectionRepo.UpdateAsync(section);
                TempData["Success"] = "Custom section content updated!";
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetToDefaults()
        {
            var defaultSettings = new Dictionary<string, string>
            {
                { "default_language", "en" },
                { "default_theme", "dark" },
                { "site_title_en", "My Portfolio" },
                { "site_title_ar", "محفظتي" },
                { "primary_color", "#667eea" },
                { "secondary_color", "#764ba2" },
                { "enable_particles", "true" },
                { "show_back_to_top", "true" },
                { "meta_description_en", "Professional Portfolio Website" },
                { "meta_description_ar", "موقع محفظة احترافي" }
            };

            var allSettings = await _settingsRepo.GetAllAsync();

            foreach (var setting in allSettings)
            {
                if (defaultSettings.ContainsKey(setting.Key))
                {
                    setting.ValueEn = defaultSettings[setting.Key];
                    setting.ValueAr = defaultSettings[setting.Key];
                    await _settingsRepo.UpdateAsync(setting);
                }
            }

            TempData["Success"] = "Settings reset to default values successfully!";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadHeroImage(IFormFile heroImage)
        {
            if (heroImage != null && heroImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "hero");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(heroImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await heroImage.CopyToAsync(fileStream);
                }

                var imageUrl = "/images/hero/" + uniqueFileName;

                // Update setting
                var setting = (await _settingsRepo.GetAllAsync())
                    .FirstOrDefault(s => s.Key == "hero_background_image");

                if (setting != null)
                {
                    // Delete old image
                    if (!string.IsNullOrEmpty(setting.ValueEn))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, setting.ValueEn.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    setting.ValueEn = imageUrl;
                    setting.ValueAr = imageUrl;
                    await _settingsRepo.UpdateAsync(setting);
                }

                TempData["Success"] = "Hero background image uploaded successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveHeroImage()
        {
            var setting = (await _settingsRepo.GetAllAsync())
                .FirstOrDefault(s => s.Key == "hero_background_image");

            if (setting != null && !string.IsNullOrEmpty(setting.ValueEn))
            {
                // Delete image
                var oldPath = Path.Combine(_env.WebRootPath, setting.ValueEn.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }

                setting.ValueEn = "";
                setting.ValueAr = "";
                await _settingsRepo.UpdateAsync(setting);

                TempData["Success"] = "Hero background image removed successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPreloaderLogo(IFormFile preloaderLogo)
        {
            if (preloaderLogo != null && preloaderLogo.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "preloader");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(preloaderLogo.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await preloaderLogo.CopyToAsync(fileStream);
                }

                var imageUrl = "/images/preloader/" + uniqueFileName;

                // Update setting
                var setting = (await _settingsRepo.GetAllAsync())
                    .FirstOrDefault(s => s.Key == "preloader_logo");

                if (setting != null)
                {
                    // Delete old logo
                    if (!string.IsNullOrEmpty(setting.ValueEn))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, setting.ValueEn.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    setting.ValueEn = imageUrl;
                    setting.ValueAr = imageUrl;
                    await _settingsRepo.UpdateAsync(setting);
                }
                else
                {
                    // Create new setting if it doesn't exist
                    await _settingsRepo.AddAsync(new SiteSettings
                    {
                        Key = "preloader_logo",
                        ValueEn = imageUrl,
                        ValueAr = imageUrl,
                        Description = "Custom logo for preloader",
                        DataType = "file"
                    });
                }

                TempData["Success"] = "Preloader logo uploaded successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePreloaderLogo()
        {
            var setting = (await _settingsRepo.GetAllAsync())
                .FirstOrDefault(s => s.Key == "preloader_logo");

            if (setting != null && !string.IsNullOrEmpty(setting.ValueEn))
            {
                // Delete logo
                var oldPath = Path.Combine(_env.WebRootPath, setting.ValueEn.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }

                setting.ValueEn = "";
                setting.ValueAr = "";
                await _settingsRepo.UpdateAsync(setting);

                TempData["Success"] = "Preloader logo removed successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPreloaderBackgroundImage(IFormFile backgroundImage)
        {
            if (backgroundImage != null && backgroundImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "preloader");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(backgroundImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await backgroundImage.CopyToAsync(fileStream);
                }

                var imageUrl = "/images/preloader/" + uniqueFileName;

                var setting = (await _settingsRepo.GetAllAsync())
                    .FirstOrDefault(s => s.Key == "preloader_background_image");

                if (setting != null)
                {
                    if (!string.IsNullOrEmpty(setting.ValueEn))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, setting.ValueEn.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }
                    setting.ValueEn = imageUrl;
                    setting.ValueAr = imageUrl;
                    await _settingsRepo.UpdateAsync(setting);
                }
                else
                {
                    await _settingsRepo.AddAsync(new SiteSettings
                    {
                        Key = "preloader_background_image",
                        ValueEn = imageUrl,
                        ValueAr = imageUrl,
                        Description = "Custom background image for preloader",
                        DataType = "file"
                    });
                }

                TempData["Success"] = "Preloader background uploaded successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePreloaderBackgroundImage()
        {
            var setting = (await _settingsRepo.GetAllAsync())
                .FirstOrDefault(s => s.Key == "preloader_background_image");

            if (setting != null && !string.IsNullOrEmpty(setting.ValueEn))
            {
                var oldPath = Path.Combine(_env.WebRootPath, setting.ValueEn.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);

                setting.ValueEn = "";
                setting.ValueAr = "";
                await _settingsRepo.UpdateAsync(setting);

                TempData["Success"] = "Preloader background removed successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}