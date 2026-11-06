using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;

namespace portfolio.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AboutController : Controller
    {
        private readonly IRepository<About> _aboutRepo;
        private readonly IRepository<TypingTitle> _titleRepo;
        private readonly IWebHostEnvironment _env;

        public AboutController(IRepository<About> aboutRepo, IRepository<TypingTitle> titleRepo, IWebHostEnvironment env)
        {
            _aboutRepo = aboutRepo;
            _titleRepo = titleRepo;
            _env = env;
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
                        about.ProfileImage = await SaveFileAsync(profileImage, "images/profile");

                    if (cvFile != null)
                        about.CVFilePath = await SaveFileAsync(cvFile, "files/cv");

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
                        DeleteFile(about.ProfileImage);
                        about.ProfileImage = await SaveFileAsync(profileImage, "images/profile");
                    }

                    if (cvFile != null)
                    {
                        DeleteFile(about.CVFilePath);
                        about.CVFilePath = await SaveFileAsync(cvFile, "files/cv");
                    }

                    await _aboutRepo.UpdateAsync(about);
                }

                TempData["Success"] = "About section updated successfully!";
                return RedirectToAction(nameof(Edit));
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

        private async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, folder);
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/" + folder + "/" + uniqueFileName;
        }

        private void DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            var fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}