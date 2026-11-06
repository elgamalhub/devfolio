using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Core.Entities
{
    public class SiteSettings
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string ValueEn { get; set; } = string.Empty;
        public string ValueAr { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DataType { get; set; } = "text"; // text, boolean, number, color
    }
}
