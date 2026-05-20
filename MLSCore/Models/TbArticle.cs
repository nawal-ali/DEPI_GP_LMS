using System.ComponentModel.DataAnnotations;

namespace MLSCore.Models
{
    public class TbArticle
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public string Content { get; set; } = "";

        [MaxLength(400)]
        public string? Excerpt { get; set; }

        public string? CoverImage { get; set; }   // relative path under Uploads/

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public int CurrentState { get; set; } = 1; // 1=active, 0=deleted
    }
}