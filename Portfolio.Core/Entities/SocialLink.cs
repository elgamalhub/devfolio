using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Core.Entities
{
    public class SocialLink
    {
        public int Id { get; set; }
        public string Platform { get; set; } = string.Empty; // GitHub, LinkedIn, Email, Phone
        public string Url { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsActive { get; set; }
    }
}
