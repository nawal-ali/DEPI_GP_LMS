using LMSProject.ViewModels.Tickets;
using Microsoft.EntityFrameworkCore;
using MLSCore.Models;
using MLSEF;

namespace LMSProject.Services
{
    /// <summary>
    /// Central service for all ticket operations.
    /// Injected into controllers across all 4 role areas.
    /// </summary>
    public class TicketService
    {
        private readonly AppDbContext _db;

        public TicketService(AppDbContext db) => _db = db;

        // ── Create ticket ─────────────────────────────────────────────────
        public async Task<TbTicket> CreateAsync(CreateTicketVM vm,
            string senderUserId, string senderName, string senderRole,
            IWebHostEnvironment env)
        {
            string? filePath = null, fileName = null;
            if (vm.Attachment != null)
                (filePath, fileName) = await SaveAttachment(vm.Attachment, env);

            var ticket = new TbTicket
            {
                Title = vm.Title,
                Description = vm.Description,
                Type = vm.Type,
                CustomType = vm.Type == "Other" ? vm.CustomType : null,
                Status = "Open",
                AttachmentPath = filePath,
                AttachmentName = fileName,
                SenderUserId = senderUserId,
                SenderName = senderName,
                SenderRole = senderRole,
                CreatedAt = DateTime.UtcNow,
                CurrentState = 1,
                IsForwarded = false
            };

            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();
            return ticket;
        }

        // ── Add reply ─────────────────────────────────────────────────────
        public async Task AddReplyAsync(ReplyTicketVM vm,
            string senderUserId, string senderName, string senderRole,
            IWebHostEnvironment env)
        {
            string? filePath = null, fileName = null;
            if (vm.Attachment != null)
                (filePath, fileName) = await SaveAttachment(vm.Attachment, env);

            _db.TicketReplies.Add(new TbTicketReply
            {
                TicketId = vm.TicketId,
                Message = vm.Message,
                SenderUserId = senderUserId,
                SenderName = senderName,
                SenderRole = senderRole,
                IsInternal = vm.IsInternal,
                AttachmentPath = filePath,
                AttachmentName = fileName,
                CreatedAt = DateTime.UtcNow,
                CurrentState = 1
            });

            var ticket = await _db.Tickets.FindAsync(vm.TicketId);
            if (ticket != null)
            {
                ticket.UpdatedAt = DateTime.UtcNow;
                if (ticket.Status == "Open") ticket.Status = "In Progress";
            }

            await _db.SaveChangesAsync();
        }

        // ── Change status ─────────────────────────────────────────────────
        public async Task ChangeStatusAsync(int ticketId, string newStatus,
            string? resolvedByUserId = null)
        {
            var t = await _db.Tickets.FindAsync(ticketId);
            if (t == null) return;
            t.Status = newStatus;
            t.UpdatedAt = DateTime.UtcNow;
            if (newStatus is "Resolved" or "Closed")
                t.ResolvedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        // ── Forward to SuperAdmin ─────────────────────────────────────────
        public async Task ForwardAsync(int ticketId, string adminUserId,
            string adminName, string? note)
        {
            var t = await _db.Tickets.FindAsync(ticketId);
            if (t == null) return;
            t.IsForwarded = true;
            t.Status = "Forwarded";
            t.ForwardedByUserId = adminUserId;
            t.ForwardedAt = DateTime.UtcNow;
            t.ForwardNote = note;
            t.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        // ── Get tickets for sender ─────────────────────────────────────────
        public async Task<List<TicketListItemVM>> GetSenderTicketsAsync(string userId)
        {
            return await _db.Tickets
                .Where(t => t.SenderUserId == userId && t.CurrentState == 1)
                .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
                .Select(t => new TicketListItemVM
                {
                    Id = t.Id,
                    Title = t.Title,
                    DisplayType = t.Type == "Other" && t.CustomType != null ? t.CustomType : t.Type,
                    Status = t.Status,
                    StatusColor = t.StatusColor,
                    StatusBg = t.StatusBg,
                    SenderName = t.SenderName,
                    SenderRole = t.SenderRole,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    ReplyCount = t.Replies.Count(r => r.CurrentState == 1),
                    IsForwarded = t.IsForwarded,
                    HasAttachment = t.AttachmentPath != null
                }).ToListAsync();
        }

        // ── Get tickets for Admin (received, non-forwarded) ────────────────
        public async Task<List<TicketListItemVM>> GetAdminTicketsAsync(string status = "")
        {
            var q = _db.Tickets
                .Where(t => !t.IsForwarded && t.CurrentState == 1);
            if (!string.IsNullOrEmpty(status)) q = q.Where(t => t.Status == status);

            return await q.OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
                .Select(t => new TicketListItemVM
                {
                    Id = t.Id,
                    Title = t.Title,
                    DisplayType = t.Type == "Other" && t.CustomType != null ? t.CustomType : t.Type,
                    Status = t.Status,
                    StatusColor = t.StatusColor,
                    StatusBg = t.StatusBg,
                    SenderName = t.SenderName,
                    SenderRole = t.SenderRole,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    ReplyCount = t.Replies.Count(r => r.CurrentState == 1),
                    IsForwarded = false,
                    HasAttachment = t.AttachmentPath != null
                }).ToListAsync();
        }

        // ── Get forwarded tickets for SuperAdmin ───────────────────────────
        public async Task<List<TicketListItemVM>> GetForwardedTicketsAsync(string status = "")
        {
            var q = _db.Tickets
                .Where(t => t.IsForwarded && t.CurrentState == 1);
            if (!string.IsNullOrEmpty(status)) q = q.Where(t => t.Status == status);

            return await q.OrderByDescending(t => t.ForwardedAt)
                .Select(t => new TicketListItemVM
                {
                    Id = t.Id,
                    Title = t.Title,
                    DisplayType = t.Type == "Other" && t.CustomType != null ? t.CustomType : t.Type,
                    Status = t.Status,
                    StatusColor = t.StatusColor,
                    StatusBg = t.StatusBg,
                    SenderName = t.SenderName,
                    SenderRole = t.SenderRole,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    ReplyCount = t.Replies.Count(r => r.CurrentState == 1),
                    IsForwarded = true,
                    HasAttachment = t.AttachmentPath != null,
                    ForwardedByName = t.ForwardedBy != null ? t.ForwardedBy.FullName : null
                }).ToListAsync();
        }

        // ── Get ticket detail ──────────────────────────────────────────────
        public async Task<TicketDetailVM> GetDetailAsync(int id,
            string currentUserId, string currentUserRole)
        {
            var ticket = await _db.Tickets
                .Include(t => t.Sender)
                .Include(t => t.ForwardedBy)
                .Include(t => t.Replies.Where(r => r.CurrentState == 1))
                .ThenInclude(r => r.Sender)
                .FirstOrDefaultAsync(t => t.Id == id && t.CurrentState == 1);

            if (ticket == null) return new TicketDetailVM { Ticket = null! };

            // Access control
            bool isSender = ticket.SenderUserId == currentUserId;
            bool isAdmin = currentUserRole is "Admin" or "SuperAdmin";
            bool isSA = currentUserRole == "SuperAdmin";
            bool canAccess = isSender || isAdmin;
            if (!canAccess) return new TicketDetailVM { Ticket = null! };

            // SuperAdmin can only see forwarded tickets
            if (isSA && !ticket.IsForwarded) return new TicketDetailVM { Ticket = null! };

            return new TicketDetailVM
            {
                Ticket = ticket,
                Replies = ticket.Replies.OrderBy(r => r.CreatedAt).ToList(),
                CurrentUserId = currentUserId,
                CurrentUserRole = currentUserRole,
                CanReply = ticket.Status is not ("Resolved" or "Closed"),
                CanChangeStatus = isAdmin,
                CanForward = currentUserRole == "Admin" && !ticket.IsForwarded &&
                                  ticket.Status != "Closed",
                CanClose = isAdmin
            };
        }

        // ── Count for notification bell ────────────────────────────────────
        public Task<int> CountNewForAdminAsync() =>
            _db.Tickets.CountAsync(t => t.Status == "Open" && !t.IsForwarded && t.CurrentState == 1);

        public Task<int> CountForwardedForSAAsync() =>
            _db.Tickets.CountAsync(t => t.IsForwarded && t.Status == "Forwarded" && t.CurrentState == 1);

        // ── Private helpers ────────────────────────────────────────────────
        private static async Task<(string path, string name)> SaveAttachment(
            IFormFile file, IWebHostEnvironment env)
        {
            var folder = Path.Combine(env.WebRootPath, "Uploads", "Tickets");
            Directory.CreateDirectory(folder);
            var uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(folder, uniqueName);
            using var stream = System.IO.File.Create(fullPath);
            await file.CopyToAsync(stream);
            return ($"Uploads/Tickets/{uniqueName}", file.FileName);
        }
    }
}