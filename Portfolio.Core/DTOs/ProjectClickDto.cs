using System.ComponentModel.DataAnnotations;

namespace Portfolio.Core.DTOs
{
    public class ProjectClickDto
    {
        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }
    }
}
