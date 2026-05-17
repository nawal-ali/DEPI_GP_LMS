using MLSCore.IdentityModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MLSCore.Models
{
    public class TbTicket
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [StringLength(50)]
        public string Type { get; set; } = "Inquiry";

        [StringLength(100)]
        public string? CustomType { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Open";

        public string? AttachmentPath { get; set; }
        public string? AttachmentName { get; set; }

        // ── Sender ────────────────────────────────────────────────────────
        // Column in DB is "SenderUserId" — tell EF explicitly so it doesn't
        // invent "SenderId" from the navigation property name
        [Required]
        [Column("SenderUserId")]
        public string SenderUserId { get; set; } = "";

        [ForeignKey("SenderUserId")]
        public ApplicationUser? Sender { get; set; }

        [StringLength(200)]
        public string SenderName { get; set; } = "";

        [StringLength(50)]
        public string SenderRole { get; set; } = "";

        // ── Assignment ────────────────────────────────────────────────────
        [Column("AssignedToUserId")]
        public string? AssignedToUserId { get; set; }

        [ForeignKey("AssignedToUserId")]
        public ApplicationUser? AssignedTo { get; set; }

        // ── Forwarding ────────────────────────────────────────────────────
        [Column("ForwardedByUserId")]
        public string? ForwardedByUserId { get; set; }

        [ForeignKey("ForwardedByUserId")]
        public ApplicationUser? ForwardedBy { get; set; }

        public DateTime? ForwardedAt { get; set; }
        public string? ForwardNote { get; set; }
        public bool IsForwarded { get; set; } = false;

        // ── Dates & state ─────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public int CurrentState { get; set; } = 1;

        // ── Navigation ────────────────────────────────────────────────────
        public List<TbTicketReply> Replies { get; set; } = new();

        // ── Computed (not mapped to DB columns) ───────────────────────────
        [NotMapped]
        public string DisplayType => Type == "Other" && !string.IsNullOrEmpty(CustomType)
            ? CustomType : Type;

        [NotMapped]
        public string StatusColor => Status switch
        {
            "Open" => "#E13468",
            "In Progress" => "#F48C06",
            "Resolved" => "#16a34a",
            "Closed" => "#6c757d",
            "Forwarded" => "#5B72EE",
            _ => "#9ca3af"
        };

        [NotMapped]
        public string StatusBg => Status switch
        {
            "Open" => "rgba(225,52,104,.12)",
            "In Progress" => "rgba(244,140,6,.12)",
            "Resolved" => "rgba(22,163,74,.12)",
            "Closed" => "rgba(108,117,125,.12)",
            "Forwarded" => "rgba(91,114,238,.12)",
            _ => "rgba(156,163,175,.12)"
        };
    }

    public class TbTicketReply
    {
        public int Id { get; set; }

        [Column("TicketId")]
        public int TicketId { get; set; }

        [ForeignKey("TicketId")]
        public TbTicket? Ticket { get; set; }

        [Required]
        public string Message { get; set; } = "";

        [Column("SenderUserId")]
        [Required]
        public string SenderUserId { get; set; } = "";

        [ForeignKey("SenderUserId")]
        public ApplicationUser? Sender { get; set; }

        [StringLength(200)]
        public string SenderName { get; set; } = "";

        [StringLength(50)]
        public string SenderRole { get; set; } = "";

        public bool IsInternal { get; set; } = false;

        public string? AttachmentPath { get; set; }
        public string? AttachmentName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CurrentState { get; set; } = 1;

        public bool IsFromSender(string ticketSenderUserId)
            => SenderUserId == ticketSenderUserId;
    }
}