using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MLSCore.Models
{
    public class TbUserAnnouncementRead
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        public int AnnouncementId { get; set; }

        public DateTime ReadAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(AnnouncementId))]
        public TbAnnouncement? Announcement { get; set; }
    }
}
