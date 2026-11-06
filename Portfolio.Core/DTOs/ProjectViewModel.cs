using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Core.DTOs
{
    public class ProjectViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Title (English)")]
        public string TitleEn { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Title (Arabic)")]
        public string TitleAr { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Short Description (English)")]
        public string ShortDescriptionEn { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Short Description (Arabic)")]
        public string ShortDescriptionAr { get; set; } = string.Empty;

        [Display(Name = "Full Description (English)")]
        public string FullDescriptionEn { get; set; } = string.Empty;

        [Display(Name = "Full Description (Arabic)")]
        public string FullDescriptionAr { get; set; } = string.Empty;

        [Display(Name = "Project Image")]
        public IFormFile? Image { get; set; }

        public string? ExistingImagePath { get; set; }

        [Display(Name = "Icon Class")]
        public string IconClass { get; set; } = "fas fa-code";

        [Display(Name = "Demo URL")]
        public string DemoUrl { get; set; } = string.Empty;

        [Display(Name = "GitHub URL")]
        public string GithubUrl { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; } = "web";

        [Display(Name = "Hide from main view")]
        public bool IsHidden { get; set; }

        [Display(Name = "Order")]
        public int Order { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Technologies (comma separated)")]
        public string Technologies { get; set; } = string.Empty;
    }
}
