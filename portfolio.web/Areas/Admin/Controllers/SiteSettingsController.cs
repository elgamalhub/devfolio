using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;
using portfolio.web.Services;

namespace Portfolio.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SiteSettingsController : Controller
    {
        private readonly IRepository<SiteSettings> _settingsRepo;
        private readonly IRepository<Section> _sectionRepo;
        private readonly IFileStorage _fileStorage;

        public SiteSettingsController(
            IRepository<SiteSettings> settingsRepo,
            IRepository<Section> sectionRepo,
            IFileStorage fileStorage)
        {
            _settingsRepo = settingsRepo;
            _sectionRepo = sectionRepo;
            _fileStorage = fileStorage;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _settingsRepo.GetAllAsync();
            ViewBag.Sections = (await _sectionRepo.GetAllAsync())
                .OrderBy(s => s.Order)
                .ThenBy(s => s.Id);
            return View(settings.OrderBy(s => s.Key));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSettings(
            [FromForm] Dictionary<string, string>? en,
            [FromForm] Dictionary<string, string>? ar)
        {
            en ??= new Dictionary<string, string>();
            ar ??= new Dictionary<string, string>();

            var allSettings = (await _settingsRepo.GetAllAsync()).ToList();
            var updated = 0;

            foreach (var setting in allSettings)
            {
                var hasEn = en.TryGetValue(setting.Key, out var enValue);
                var hasAr = ar.TryGetValue(setting.Key, out var arValue);

                if (!hasEn && !hasAr)
                {
                    continue;
                }

                if (hasEn)
                {
                    setting.ValueEn = (enValue ?? string.Empty).Trim();
                }

                if (hasAr)
                {
                    setting.ValueAr = (arValue ?? string.Empty).Trim();
                }
                else if (hasEn)
                {
                    // Non-translatable settings (colours, toggles, preloader options)
                    // keep the Arabic column in sync with the single stored value.
                    setting.ValueAr = setting.ValueEn;
                }

                await _settingsRepo.UpdateAsync(setting);
                updated++;
            }

            TempData["Success"] = updated > 0
                ? "Settings updated successfully."
                : "No settings were submitted.";
            return RedirectToAction(nameof(Index));
        }

        // Section management -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSection(Section model)
        {
            ModelState.Remove(nameof(Section.CustomContent));

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please provide valid section details.";
                return RedirectToAction(nameof(Index));
            }

            var existing = (await _sectionRepo.GetAllAsync()).ToList();
            if (existing.Any(s => string.Equals(s.Identifier, model.Identifier, StringComparison.OrdinalIgnoreCase)))
            {
                TempData["Error"] = $"A section with identifier '{model.Identifier}' already exists.";
                return RedirectToAction(nameof(Index));
            }

            model.Order = existing.Count == 0 ? 1 : existing.Max(s => s.Order) + 1;
            model.IsBuiltIn = false;
            model.CreatedAt = DateTime.UtcNow;

            await _sectionRepo.AddAsync(model);
            TempData["Success"] = "Section added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSection(
            int id,
            string nameEn,
            string nameAr,
            string iconClass,
            bool isVisible,
            bool showInNavbar)
        {
            var section = await _sectionRepo.GetByIdAsync(id);
            if (section == null)
            {
                TempData["Error"] = "Section not found.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(nameEn) || string.IsNullOrWhiteSpace(nameAr))
            {
                TempData["Error"] = "Both English and Arabic names are required.";
                return RedirectToAction(nameof(Index));
            }

            section.NameEn = nameEn.Trim();
            section.NameAr = nameAr.Trim();
            section.IconClass = string.IsNullOrWhiteSpace(iconClass) ? "fas fa-section" : iconClass.Trim();
            section.IsVisible = isVisible;
            section.ShowInNavbar = showInNavbar;

            await _sectionRepo.UpdateAsync(section);
            TempData["Success"] = "Section updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveSection(int id, string direction)
        {
            var sections = (await _sectionRepo.GetAllAsync())
                .OrderBy(s => s.Order)
                .ThenBy(s => s.Id)
                .ToList();

            var index = sections.FindIndex(s => s.Id == id);
            if (index < 0)
            {
                TempData["Error"] = "Section not found.";
                return RedirectToAction(nameof(Index));
            }

            var target = string.Equals(direction, "up", StringComparison.OrdinalIgnoreCase)
                ? index - 1
                : index + 1;

            if (target < 0 || target >= sections.Count)
            {
                return RedirectToAction(nameof(Index));
            }

            (sections[index], sections[target]) = (sections[target], sections[index]);

            for (var i = 0; i < sections.Count; i++)
            {
                sections[i].Order = i + 1;
                await _sectionRepo.UpdateAsync(sections[i]);
            }

            TempData["Success"] = "Section order updated.";
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
                TempData["Success"] = "Section deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Built-in sections cannot be deleted.";
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
                TempData["Success"] = "Section visibility updated.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditCustomSection(int id)
        {
            var section = await _sectionRepo.GetByIdAsync(id);
            if (section == null || section.IsBuiltIn)
            {
                return NotFound();
            }

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
                TempData["Success"] = "Custom section content updated.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Defaults -----------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetToDefaults()
        {
            var defaults = new Dictionary<string, (string En, string Ar)>
            {
                ["default_language"] = ("en", "en"),
                ["default_theme"] = ("dark", "dark"),
                ["site_title_en"] = ("Ahmed Elgamal", "Ahmed Elgamal"),
                ["site_title_ar"] = ("أحمد الجمال", "أحمد الجمال"),
                ["primary_color"] = ("#667eea", "#667eea"),
                ["secondary_color"] = ("#764ba2", "#764ba2"),
                ["enable_particles"] = ("true", "true"),
                ["enable_preloader"] = ("false", "false"),
                ["show_back_to_top"] = ("true", "true"),
                ["preloader_style"] = ("spinner", "spinner"),
                ["preloader_color"] = ("#667eea", "#667eea"),
                ["preloader_duration"] = ("600", "600"),
                ["preloader_background"] = ("gradient", "gradient"),
                ["meta_description_en"] = (
                    "Full Stack .NET developer building secure, polished web applications.",
                    "Full Stack .NET developer building secure, polished web applications."),
                ["meta_description_ar"] = (
                    "مطور .NET متكامل يبني تطبيقات ويب آمنة ومتقنة.",
                    "مطور .NET متكامل يبني تطبيقات ويب آمنة ومتقنة.")
            };

            var allSettings = await _settingsRepo.GetAllAsync();

            foreach (var setting in allSettings)
            {
                if (!defaults.TryGetValue(setting.Key, out var value))
                {
                    continue;
                }

                setting.ValueEn = value.En;
                setting.ValueAr = value.Ar;
                await _settingsRepo.UpdateAsync(setting);
            }

            TempData["Success"] = "Settings restored to their default values.";
            return RedirectToAction(nameof(Index));
        }

        // Image settings -----------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> UploadHeroImage(IFormFile? heroImage)
        {
            return UploadImageSettingAsync(
                heroImage,
                "images/hero",
                "hero_background_image",
                "Hero background image uploaded successfully.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> RemoveHeroImage()
        {
            return RemoveImageSettingAsync("hero_background_image", "Hero background image removed.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> UploadPreloaderLogo(IFormFile? preloaderLogo)
        {
            return UploadImageSettingAsync(
                preloaderLogo,
                "images/preloader",
                "preloader_logo",
                "Preloader logo uploaded successfully.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> RemovePreloaderLogo()
        {
            return RemoveImageSettingAsync("preloader_logo", "Preloader logo removed.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> UploadPreloaderBackgroundImage(IFormFile? backgroundImage)
        {
            return UploadImageSettingAsync(
                backgroundImage,
                "images/preloader",
                "preloader_background_image",
                "Preloader background uploaded successfully.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> RemovePreloaderBackgroundImage()
        {
            return RemoveImageSettingAsync("preloader_background_image", "Preloader background removed.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> UploadOgImage(IFormFile? ogImage)
        {
            return UploadImageSettingAsync(
                ogImage,
                "images/social",
                "og_image",
                "Social sharing image uploaded successfully.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> RemoveOgImage()
        {
            return RemoveImageSettingAsync("og_image", "Social sharing image removed.");
        }

        private async Task<IActionResult> UploadImageSettingAsync(
            IFormFile? file,
            string folder,
            string settingKey,
            string successMessage)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please choose an image to upload.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var imageUrl = await _fileStorage.SaveAsync(file, folder, FileUploadKind.Image);
                var setting = (await _settingsRepo.GetAllAsync())
                    .FirstOrDefault(s => s.Key == settingKey);

                if (setting != null)
                {
                    _fileStorage.Delete(setting.ValueEn);
                    setting.ValueEn = imageUrl;
                    setting.ValueAr = imageUrl;
                    setting.DataType = "file";
                    await _settingsRepo.UpdateAsync(setting);
                }
                else
                {
                    await _settingsRepo.AddAsync(new SiteSettings
                    {
                        Key = settingKey,
                        ValueEn = imageUrl,
                        ValueAr = imageUrl,
                        Description = settingKey.Replace('_', ' '),
                        DataType = "file"
                    });
                }

                TempData["Success"] = successMessage;
            }
            catch (FileValidationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> RemoveImageSettingAsync(string settingKey, string successMessage)
        {
            var setting = (await _settingsRepo.GetAllAsync())
                .FirstOrDefault(s => s.Key == settingKey);

            if (setting != null && !string.IsNullOrEmpty(setting.ValueEn))
            {
                _fileStorage.Delete(setting.ValueEn);
                setting.ValueEn = string.Empty;
                setting.ValueAr = string.Empty;
                await _settingsRepo.UpdateAsync(setting);
                TempData["Success"] = successMessage;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
