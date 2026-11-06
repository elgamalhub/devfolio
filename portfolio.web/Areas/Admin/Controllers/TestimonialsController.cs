using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;

namespace portfolio.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TestimonialsController : Controller
    {
        private readonly IRepository<Testimonial> _testimonialRepo;
        private readonly IWebHostEnvironment _env;

        public TestimonialsController(IRepository<Testimonial> testimonialRepo, IWebHostEnvironment env)
        {
            _testimonialRepo = testimonialRepo;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var testimonials = (await _testimonialRepo.GetAllAsync()).OrderBy(t => t.Order);
            return View(testimonials);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Testimonial model, IFormFile? clientImage)
        {
            if (ModelState.IsValid)
            {
                if (clientImage != null)
                    model.ClientImage = await SaveImageAsync(clientImage);

                await _testimonialRepo.AddAsync(model);
                TempData["Success"] = "Testimonial created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var testimonial = await _testimonialRepo.GetByIdAsync(id);
            if (testimonial == null)
                return NotFound();

            return View(testimonial);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Testimonial model, IFormFile? clientImage)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var testimonial = await _testimonialRepo.GetByIdAsync(id);
                if (testimonial == null)
                    return NotFound();

                testimonial.ClientName = model.ClientName;
                testimonial.Position = model.Position;
                testimonial.Company = model.Company;
                testimonial.TestimonialTextEn = model.TestimonialTextEn;
                testimonial.TestimonialTextAr = model.TestimonialTextAr;
                testimonial.Rating = model.Rating;
                testimonial.IsActive = model.IsActive;
                testimonial.Order = model.Order;

                if (clientImage != null)
                {
                    DeleteImage(testimonial.ClientImage);
                    testimonial.ClientImage = await SaveImageAsync(clientImage);
                }

                await _testimonialRepo.UpdateAsync(testimonial);
                TempData["Success"] = "Testimonial updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var testimonial = await _testimonialRepo.GetByIdAsync(id);
            if (testimonial != null)
            {
                DeleteImage(testimonial.ClientImage);
                await _testimonialRepo.DeleteAsync(id);
                TempData["Success"] = "Testimonial deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "testimonials");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/images/testimonials/" + uniqueFileName;
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