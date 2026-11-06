using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Core.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string ShortDescriptionEn { get; set; } = string.Empty;
        public string ShortDescriptionAr { get; set; } = string.Empty;
        public string FullDescriptionEn { get; set; } = string.Empty;
        public string FullDescriptionAr { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string IconClass { get; set; } = "fas fa-code";
        public string DemoUrl { get; set; } = string.Empty;
        public string GithubUrl { get; set; } = string.Empty;
        public string Category { get; set; } = "web";
        public bool IsHidden { get; set; } // for show more functionality
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<ProjectTechnology> Technologies { get; set; } = new List<ProjectTechnology>();
    }
}
