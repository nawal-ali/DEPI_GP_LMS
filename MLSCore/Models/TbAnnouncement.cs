using MLSCore.IdentityModel;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MLSCore.Models
{
    public class TbAnnouncement
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        // ── All optional strings marked nullable so EF never throws on NULL columns ──
        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string TargetAudience { get; set; } = "All";

        [StringLength(20)]
        public string? Priority { get; set; } = "Medium";

        [StringLength(50)]
        public string? Category { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(500)]
        public string? AttachmentUrl { get; set; }

        public DateTime PublishedDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsPinned { get; set; } = false;

        public bool RequiresAcknowledgment { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;

        public int CurrentState { get; set; } = 1;

        [StringLength(256)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(256)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [ForeignKey("Creator")]
        [StringLength(256)]
        public string? CreatedByUserId { get; set; }

        public ApplicationUser? Creator { get; set; }

        public int ViewCount { get; set; } = 0;

        [ForeignKey("Term")]
        public int? TermId { get; set; }
        public TbTerm? Term { get; set; }

        [ForeignKey("Course")]
        public int? CourseId { get; set; }
        public TbCourse? Course { get; set; }

        [ForeignKey("Grade")]
        public int? GradeId { get; set; }
        public TbGrade? Grade { get; set; }
    }
}