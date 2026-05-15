using Microsoft.AspNetCore.Identity;
using MLSCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLSCore.IdentityModel
{
    public class ApplicationUser : IdentityUser
    {
        // Nullable — these columns can be NULL in the DB for seeded/legacy accounts
        public string? FullName { get; set; }
        public string? ProfileImage { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>Navigation property to Instructor (1:1) if role is Instructor</summary>
        public TbInstructor? Instructor { get; set; }

        /// <summary>Navigation property to Student (1:1) if role is Student</summary>
        public TbStudent? Student { get; set; }

        /// <summary>Navigation property to Parent (1:1) if role is Parent</summary>
        public TbParent? Parent { get; set; }

        /// <summary>Announcements created by this user (Admin/SuperAdmin)</summary>
        public ICollection<TbAnnouncement> CreatedAnnouncements { get; set; } = new List<TbAnnouncement>();
    }
}