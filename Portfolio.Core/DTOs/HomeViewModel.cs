using Portfolio.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Core.DTOs
{
    public class HomeViewModel
    {
        public About? About { get; set; }
        public IEnumerable<Skill> Skills { get; set; } = new List<Skill>();
        public IEnumerable<Experience> Experiences { get; set; } = new List<Experience>();
        public IEnumerable<Project> Projects { get; set; } = new List<Project>();
        public IEnumerable<SocialLink> SocialLinks { get; set; } = new List<SocialLink>();
        public IEnumerable<TypingTitle> TypingTitles { get; set; } = new List<TypingTitle>();
        public IEnumerable<Testimonial> Testimonials { get; set; } = new List<Testimonial>();
        public IEnumerable<Certificate> Certificates { get; set; } = new List<Certificate>();
        public string CurrentLanguage { get; set; } = "en";
    }
}
