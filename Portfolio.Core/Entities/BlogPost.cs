using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Core.Entities
{
    public class BlogPost
    {
        public int Id { get; set; }
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string ContentEn { get; set; } = string.Empty;
        public string ContentAr { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string FeaturedImage { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public bool IsPublished { get; set; }
        public int ViewCount { get; set; }
        public ICollection<BlogTag> Tags { get; set; } = new List<BlogTag>();
    }
}
