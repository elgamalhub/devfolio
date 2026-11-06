using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Portfolio.Core.DTOs
{
    public class CertificateViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Certificate name (English)")]
        public string NameEn { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Certificate name (Arabic)")]
        public string NameAr { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Issuer")]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Issue date")]
        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Credential URL")]
        [Url]
        public string CertificateUrl { get; set; } = string.Empty;

        [Display(Name = "Badge image")]
        public IFormFile? BadgeImageFile { get; set; }

        public string? ExistingBadgeImage { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Order")]
        public int Order { get; set; }
    }
}
