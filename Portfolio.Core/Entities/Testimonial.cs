using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Core.Entities
{
    public class Testimonial
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string TestimonialTextEn { get; set; } = string.Empty;
        public string TestimonialTextAr { get; set; } = string.Empty;
        public string ClientImage { get; set; } = string.Empty;
        public int Rating { get; set; } = 5;
        public bool IsActive { get; set; } = true;
        public int Order { get; set; }
    }
}
