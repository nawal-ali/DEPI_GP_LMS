using MLSCore.IdentityModel;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MLSCore.Models
{
    /// <summary>
    /// TbParent: Represents a parent/guardian user in the LMS system.
    /// A parent can have multiple children (students) and access their academic progress.
    /// Only SuperAdmin can create parent accounts.
    /// </summary>
    public class TbParent
    {
        public int Id { get; set; }

        /// <summary>
        /// Full name of the parent/guardian
        /// </summary>
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        /// <summary>
        /// Relationship to the student (e.g., "Father", "Mother", "Guardian")
        /// </summary>
        [StringLength(50)]
        public string Relationship { get; set; }

        /// <summary>
        /// Primary phone number of the parent
        /// </summary>
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Alternative phone number (optional)
        /// </summary>
        [StringLength(20)]
        public string AlternativePhoneNumber { get; set; }

        /// <summary>
        /// Email address (can be same as ApplicationUser email)
        /// </summary>
        [StringLength(256)]
        public string Email { get; set; }

        /// <summary>
        /// National ID/Passport number for verification
        /// </summary>
        [StringLength(50)]
        public string NationalId { get; set; }

        /// <summary>
        /// Profile image name/path
        /// </summary>
        [StringLength(255)]
        public string ImageName { get; set; }

        /// <summary>
        /// Whether this parent is the primary guardian (used for contact)
        /// </summary>
        public bool IsPrimaryGuardian { get; set; } = true;

        /// <summary>
        /// Address of the parent/household
        /// </summary>
        [StringLength(500)]
        public string Address { get; set; }

        /// <summary>
        /// City/Region
        /// </summary>
        [StringLength(100)]
        public string City { get; set; }

        /// <summary>
        /// Postal code
        /// </summary>
        [StringLength(20)]
        public string PostalCode { get; set; }

        /// <summary>
        /// Occupation of the parent
        /// </summary>
        [StringLength(100)]
        public string Occupation { get; set; }

        /// <summary>
        /// Current state (1=Active, 0=Inactive/Deleted)
        /// </summary>
        public int CurrentState { get; set; } = 1;

        /// <summary>
        /// Audit: Who created this record
        /// </summary>
        [StringLength(256)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Audit: When this record was created
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// Audit: Who last updated this record
        /// </summary>
        [StringLength(256)]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Audit: When this record was last updated
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Foreign key to ApplicationUser (Identity)
        /// </summary>
        [ForeignKey("User")]
        public string UserId { get; set; }

        /// <summary>
        /// Navigation property to ApplicationUser
        /// </summary>
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Navigation property to children (students) - One-to-Many
        /// A parent can have multiple children (students)
        /// </summary>
        public ICollection<TbStudent> Children { get; set; } = new List<TbStudent>();
    }
}
