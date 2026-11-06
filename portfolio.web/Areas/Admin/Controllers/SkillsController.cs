using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;

namespace portfolio.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SkillsController : Controller
    {
        private readonly IRepository<Skill> _skillRepo;

        public SkillsController(IRepository<Skill> skillRepo)
        {
            _skillRepo = skillRepo;
        }

        public async Task<IActionResult> Index()
        {
            var skills = (await _skillRepo.GetAllAsync()).OrderBy(s => s.Order);
            return View(skills);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Skill skill)
        {
            if (ModelState.IsValid)
            {
                await _skillRepo.AddAsync(skill);
                TempData["Success"] = "Skill created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(skill);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var skill = await _skillRepo.GetByIdAsync(id);
            if (skill == null)
                return NotFound();

            return View(skill);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Skill skill)
        {
            if (id != skill.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                await _skillRepo.UpdateAsync(skill);
                TempData["Success"] = "Skill updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(skill);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _skillRepo.DeleteAsync(id);
            TempData["Success"] = "Skill deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}