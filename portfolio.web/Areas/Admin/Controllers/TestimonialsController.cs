using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;
using portfolio.web.Services;

namespace portfolio.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TestimonialsController : Controller
    {
        private readonly IRepository<Testimonial> _testimonialRepo;
        private readonly IFileStorage _fileStorage;

        public TestimonialsController(IRepository<Testimonial> testimonialRepo, IFileStorage fileStorage)
        {
            _testimonialRepo = testimonialRepo;
            _fileStorage = fileStorage;
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
                try
                {
                    if (clientImage != null)
                    {
                        model.ClientImage = await _fileStorage.SaveAsync(
                            clientImage, "images/testimonials", FileUploadKind.Image);
                    }

                    await _testimonialRepo.AddAsync(model);
                    TempData["Success"] = "Testimonial created successfully!";
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
                try
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
                        var newImage = await _fileStorage.SaveAsync(
                            clientImage, "images/testimonials", FileUploadKind.Image);
                        _fileStorage.Delete(testimonial.ClientImage);
                        testimonial.ClientImage = newImage;
                    }

                    await _testimonialRepo.UpdateAsync(testimonial);
                    TempData["Success"] = "Testimonial updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (FileValidationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
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
                _fileStorage.Delete(testimonial.ClientImage);
                await _testimonialRepo.DeleteAsync(id);
                TempData["Success"] = "Testimonial deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
