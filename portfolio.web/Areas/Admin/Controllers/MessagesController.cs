using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;

namespace portfolio.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MessagesController : Controller
    {
        private readonly IRepository<ContactMessage> _messageRepo;

        public MessagesController(IRepository<ContactMessage> messageRepo)
        {
            _messageRepo = messageRepo;
        }

        public async Task<IActionResult> Index()
        {
            var messages = (await _messageRepo.GetAllAsync()).OrderByDescending(m => m.ReceivedAt);
            return View(messages);
        }

        public async Task<IActionResult> Details(int id)
        {
            var message = await _messageRepo.GetByIdAsync(id);
            if (message == null)
                return NotFound();

            // Mark as read
            if (!message.IsRead)
            {
                message.IsRead = true;
                await _messageRepo.UpdateAsync(message);
            }

            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int id, string note)
        {
            var message = await _messageRepo.GetByIdAsync(id);
            if (message == null)
                return NotFound();

            message.AdminNotes = note;
            await _messageRepo.UpdateAsync(message);

            TempData["Success"] = "Note added successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _messageRepo.DeleteAsync(id);
            TempData["Success"] = "Message deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await _messageRepo.GetByIdAsync(id);
            if (message != null)
            {
                message.IsRead = true;
                await _messageRepo.UpdateAsync(message);
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}