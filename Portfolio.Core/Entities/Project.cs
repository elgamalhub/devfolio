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
        public string TitleEn { get; set; }
        public string TitleAr { get; set; }
        public string ShortDescriptionEn { get; set; }
        public string ShortDescriptionAr { get; set; }
        public string FullDescriptionEn { get; set; }
        public string FullDescriptionAr { get; set; }
        public string ImagePath { get; set; }
        public string IconClass { get; set; }
        public string DemoUrl { get; set; }
        public string GithubUrl { get; set; }
        public string Category { get; set; } // web, desktop, other
        public bool IsHidden { get; set; } // for show more functionality
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<ProjectTechnology> Technologies { get; set; }
    }
}
