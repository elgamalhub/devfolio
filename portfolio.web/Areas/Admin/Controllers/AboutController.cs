using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;
using portfolio.web.Services;

namespace portfolio.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AboutController : Controller
    {
        private readonly IRepository<About> _aboutRepo;
        private readonly IRepository<TypingTitle> _titleRepo;
        private readonly IFileStorage _fileStorage;

        public AboutController(
            IRepository<About> aboutRepo,
            IRepository<TypingTitle> titleRepo,
            IFileStorage fileStorage)
        {
            _aboutRepo = aboutRepo;
            _titleRepo = titleRepo;
            _fileStorage = fileStorage;
        }

        public async Task<IActionResult> Edit()
        {
            var about = (await _aboutRepo.GetAllAsync()).FirstOrDefault();
            if (about == null)
            {
                about = new About { Id = 0 };
            }

            ViewBag.TypingTitles = await _titleRepo.GetAllAsync();
            return View(about);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(About model, IFormFile? profileImage, IFormFile? cvFile)
        {
            // Remove validation for fields we don't need
            ModelState.Remove("profileImage");
            ModelState.Remove("cvFile");

            if (ModelState.IsValid)
            {
                try
                {
                    var about = (await _aboutRepo.GetAllAsync()).FirstOrDefault();

                    if (about == null)
                    {
                        // Create new
                        about = new About
                        {
                            NameEn = model.NameEn,
                            NameAr = model.NameAr,
                            DescriptionEn = model.DescriptionEn,
                            DescriptionAr = model.DescriptionAr,
                            Email = model.Email,
                            Phone = model.Phone,
                            Location = model.Location,
                            GitHubUrl = model.GitHubUrl ?? "",
                            LinkedInUrl = model.LinkedInUrl ?? "",
                            UpdatedAt = DateTime.UtcNow
                        };

                        if (profileImage != null)
                        {
                            about.ProfileImage = await _fileStorage.SaveAsync(
                                profileImage, "images/profile", FileUploadKind.Image);
                        }

                        if (cvFile != null)
                        {
                            about.CVFilePath = await _fileStorage.SaveAsync(
                                cvFile, "files/cv", FileUploadKind.Document);
                        }

                        await _aboutRepo.AddAsync(about);
                    }
                    else
                    {
                        // Update existing
                        about.NameEn = model.NameEn;
                        about.NameAr = model.NameAr;
                        about.DescriptionEn = model.DescriptionEn;
                        about.DescriptionAr = model.DescriptionAr;
                        about.Email = model.Email;
                        about.Phone = model.Phone;
                        about.Location = model.Location;
                        about.GitHubUrl = model.GitHubUrl ?? "";
                        about.LinkedInUrl = model.LinkedInUrl ?? "";
                        about.UpdatedAt = DateTime.UtcNow;

                        if (profileImage != null)
                        {
                            var newImage = await _fileStorage.SaveAsync(
                                profileImage, "images/profile", FileUploadKind.Image);
                            _fileStorage.Delete(about.ProfileImage);
                            about.ProfileImage = newImage;
                        }

                        if (cvFile != null)
                        {
                            var newCv = await _fileStorage.SaveAsync(
                                cvFile, "files/cv", FileUploadKind.Document);
                            _fileStorage.Delete(about.CVFilePath);
                            about.CVFilePath = newCv;
                        }

                        await _aboutRepo.UpdateAsync(about);
                    }

                    TempData["Success"] = "About section updated successfully!";
                    return RedirectToAction(nameof(Edit));
                }
                catch (FileValidationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            ViewBag.TypingTitles = await _titleRepo.GetAllAsync();
            return View(model);
        }

        // Typing Titles Management
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTitle(string titleEn, string titleAr)
        {
            if (!string.IsNullOrWhiteSpace(titleEn) && !string.IsNullOrWhiteSpace(titleAr))
            {
                var maxOrder = (await _titleRepo.GetAllAsync()).Any()
                    ? (await _titleRepo.GetAllAsync()).Max(t => t.Order)
                    : 0;

                var title = new TypingTitle
                {
                    TitleEn = titleEn,
                    TitleAr = titleAr,
                    Order = maxOrder + 1,
                    IsActive = true
                };

                await _titleRepo.AddAsync(title);
                TempData["Success"] = "Typing title added successfully!";
            }

            return RedirectToAction(nameof(Edit));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTitle(int id)
        {
            await _titleRepo.DeleteAsync(id);
            TempData["Success"] = "Typing title deleted successfully!";
            return RedirectToAction(nameof(Edit));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTitleStatus(int id)
        {
            var title = await _titleRepo.GetByIdAsync(id);
            if (title != null)
            {
                title.IsActive = !title.IsActive;
                await _titleRepo.UpdateAsync(title);
                TempData["Success"] = "Title status updated successfully!";
            }

            return RedirectToAction(nameof(Edit));
        }
    }
}