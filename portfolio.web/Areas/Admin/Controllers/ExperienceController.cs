using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;

namespace portfolio.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ExperienceController : Controller
    {
        private readonly IRepository<Experience> _expRepo;

        public ExperienceController(IRepository<Experience> expRepo)
        {
            _expRepo = expRepo;
        }

        public async Task<IActionResult> Index()
        {
            var experiences = (await _expRepo.GetAllAsync()).OrderByDescending(e => e.StartDate);
            return View(experiences);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Experience experience)
        {
            if (ModelState.IsValid)
            {
                await _expRepo.AddAsync(experience);
                TempData["Success"] = "Experience created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(experience);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var experience = await _expRepo.GetByIdAsync(id);
            if (experience == null)
                return NotFound();

            return View(experience);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Experience experience)
        {
            if (id != experience.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                await _expRepo.UpdateAsync(experience);
                TempData["Success"] = "Experience updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(experience);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _expRepo.DeleteAsync(id);
            TempData["Success"] = "Experience deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}