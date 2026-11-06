using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Core.Entities
{
    public class Certificate
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public string CertificateUrl { get; set; } = string.Empty;
        public string BadgeImage { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int Order { get; set; }
    }
}
