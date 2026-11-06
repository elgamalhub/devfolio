using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Core.Entities
{
    public class Section
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty; // e.g., "about", "skills", "projects"
        public string IconClass { get; set; } = "fas fa-section";
        public int Order { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsBuiltIn { get; set; } = false; // Can't delete built-in sections
        public bool ShowInNavbar { get; set; } = true;
        public string? CustomContent { get; set; } // For custom sections
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}