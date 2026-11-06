namespace Portfolio.Core.Entities
{
    public class ProjectCategory
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty; // e.g., "web", "mobile", "desktop"
        public string IconClass { get; set; } = "fas fa-folder";
        public int Order { get; set; }
        public bool IsActive { get; set; } = true;
    }
}