using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Core.Entities
{
    public class PageView
    {
        public int Id { get; set; }
        public string PageUrl { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }
}
