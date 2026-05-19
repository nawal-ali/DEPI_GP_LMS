using System.ComponentModel.DataAnnotations;

namespace MLSCore.Models
{
    public class TbContactForm
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = "";

        [Required, MaxLength(150)]
        public string Email { get; set; } = "";

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required, MaxLength(2000)]
        public string Message { get; set; } = "";

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        /// <summary>false = unread (new), true = viewed by SuperAdmin</summary>
        public bool IsRead { get; set; } = false;
    }
}