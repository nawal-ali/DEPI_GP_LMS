using MLSCore.Models;
using System.ComponentModel.DataAnnotations;

namespace LMSProject.ViewModels.Tickets
{
    // ── Create ────────────────────────────────────────────────────────────────
    public class CreateTicketVM
    {
        [Required, StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        public string Type { get; set; } = "Inquiry";

        public string? CustomType { get; set; }

        public IFormFile? Attachment { get; set; }
    }

    // ── List item ─────────────────────────────────────────────────────────────
    public class TicketListItemVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string DisplayType { get; set; } = "";
        public string Status { get; set; } = "";
        public string StatusColor { get; set; } = "";
        public string StatusBg { get; set; } = "";
        public string SenderName { get; set; } = "";
        public string SenderRole { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ReplyCount { get; set; }
        public bool IsForwarded { get; set; }
        public bool HasAttachment { get; set; }
        public string? ForwardedByName { get; set; }
    }

    // ── Ticket detail (with conversation thread) ──────────────────────────────
    public class TicketDetailVM
    {
        public TbTicket Ticket { get; set; } = null!;
        public List<TbTicketReply> Replies { get; set; } = new();
        public string CurrentUserId { get; set; } = "";
        public string CurrentUserRole { get; set; } = "";
        public bool CanReply { get; set; } = true;
        public bool CanChangeStatus { get; set; } = false;
        public bool CanForward { get; set; } = false;
        public bool CanClose { get; set; } = false;
    }

    // ── Reply ─────────────────────────────────────────────────────────────────
    public class ReplyTicketVM
    {
        [Required]
        public int TicketId { get; set; }

        [Required]
        public string Message { get; set; } = "";

        public bool IsInternal { get; set; } = false;
        public IFormFile? Attachment { get; set; }
    }

    // ── Status change ─────────────────────────────────────────────────────────
    public class ChangeStatusVM
    {
        [Required] public int TicketId { get; set; }
        [Required] public string NewStatus { get; set; } = "";
    }

    // ── Forward ───────────────────────────────────────────────────────────────
    public class ForwardTicketVM
    {
        [Required] public int TicketId { get; set; }
        public string? ForwardNote { get; set; }
    }
}