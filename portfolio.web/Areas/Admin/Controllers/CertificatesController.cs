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
    public class CertificatesController : Controller
    {
        private readonly IRepository<Certificate> _certificateRepo;
        private readonly IFileStorage _fileStorage;

        public CertificatesController(
            IRepository<Certificate> certificateRepo,
            IFileStorage fileStorage)
        {
            _certificateRepo = certificateRepo;
            _fileStorage = fileStorage;
        }

        public async Task<IActionResult> Index()
        {
            var certificates = (await _certificateRepo.GetAllAsync())
                .OrderBy(c => c.Order)
                .ThenByDescending(c => c.IssueDate);
            return View(certificates);
        }

        public async Task<IActionResult> Create()
        {
            var certificates = await _certificateRepo.GetAllAsync();
            var model = new CertificateViewModel
            {
                Order = certificates.Any() ? certificates.Max(c => c.Order) + 1 : 1
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CertificateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var certificate = new Certificate
                {
                    NameEn = model.NameEn.Trim(),
                    NameAr = model.NameAr.Trim(),
                    Issuer = model.Issuer.Trim(),
                    IssueDate = model.IssueDate,
                    CertificateUrl = model.CertificateUrl?.Trim() ?? string.Empty,
                    IsActive = model.IsActive,
                    Order = model.Order
                };

                if (model.BadgeImageFile != null)
                {
                    certificate.BadgeImage = await _fileStorage.SaveAsync(
                        model.BadgeImageFile, "images/certificates", FileUploadKind.Image);
                }

                await _certificateRepo.AddAsync(certificate);
                TempData["Success"] = "Certificate created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (FileValidationException ex)
            {
                ModelState.AddModelError(nameof(model.BadgeImageFile), ex.Message);
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var certificate = await _certificateRepo.GetByIdAsync(id);
            if (certificate == null)
            {
                return NotFound();
            }

            return View(new CertificateViewModel
            {
                Id = certificate.Id,
                NameEn = certificate.NameEn,
                NameAr = certificate.NameAr,
                Issuer = certificate.Issuer,
                IssueDate = certificate.IssueDate,
                CertificateUrl = certificate.CertificateUrl,
                ExistingBadgeImage = certificate.BadgeImage,
                IsActive = certificate.IsActive,
                Order = certificate.Order
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CertificateViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var certificate = await _certificateRepo.GetByIdAsync(id);
            if (certificate == null)
            {
                return NotFound();
            }

            try
            {
                if (model.BadgeImageFile != null)
                {
                    var newImage = await _fileStorage.SaveAsync(
                        model.BadgeImageFile, "images/certificates", FileUploadKind.Image);
                    _fileStorage.Delete(certificate.BadgeImage);
                    certificate.BadgeImage = newImage;
                }

                certificate.NameEn = model.NameEn.Trim();
                certificate.NameAr = model.NameAr.Trim();
                certificate.Issuer = model.Issuer.Trim();
                certificate.IssueDate = model.IssueDate;
                certificate.CertificateUrl = model.CertificateUrl?.Trim() ?? string.Empty;
                certificate.IsActive = model.IsActive;
                certificate.Order = model.Order;

                await _certificateRepo.UpdateAsync(certificate);
                TempData["Success"] = "Certificate updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (FileValidationException ex)
            {
                ModelState.AddModelError(nameof(model.BadgeImageFile), ex.Message);
                model.ExistingBadgeImage = certificate.BadgeImage;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var certificate = await _certificateRepo.GetByIdAsync(id);
            if (certificate == null)
            {
                TempData["Error"] = "Certificate not found.";
                return RedirectToAction(nameof(Index));
            }

            _fileStorage.Delete(certificate.BadgeImage);
            await _certificateRepo.DeleteAsync(id);
            TempData["Success"] = "Certificate deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
