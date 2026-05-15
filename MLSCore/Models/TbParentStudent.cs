using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MLSCore.Models
{
    /// <summary>
    /// TbParentStudent: Junction table for Many-to-Many relationship between Parents and Students.
    /// A student can have multiple parents/guardians (e.g., mother, father, legal guardian).
    /// A parent can have multiple children.
    /// 
    /// Example:
    /// - Mother linked to Child 1 and Child 2
    /// - Father linked to Child 1 and Child 2
    /// - Both can view their children's progress
    /// </summary>
    public class TbParentStudent
    {
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to Parent
        /// </summary>
        [ForeignKey("Parent")]
        public int ParentId { get; set; }

        /// <summary>
        /// Navigation property to Parent
        /// </summary>
        public TbParent Parent { get; set; }

        /// <summary>
        /// Foreign key to Student
        /// </summary>
        [ForeignKey("Student")]
        public int StudentId { get; set; }

        /// <summary>
        /// Navigation property to Student
        /// </summary>
        public TbStudent Student { get; set; }

        /// <summary>
        /// Type of relationship: "Mother", "Father", "Guardian", "Uncle", "Aunt", etc.
        /// </summary>
        [StringLength(50)]
        public string RelationshipType { get; set; } = "Guardian";

        /// <summary>
        /// Whether this parent/guardian has legal custody/guardianship
        /// </summary>
        public bool IsGuardian { get; set; } = true;

        /// <summary>
        /// Whether this parent should receive school communications/notifications
        /// </summary>
        public bool ReceiveNotifications { get; set; } = true;

        /// <summary>
        /// Permission level: 
        /// 1 = Can view grades, attendance, progress
        /// 2 = Can view + comment on assignments
        /// 3 = Full access (admin-level parent)
        /// </summary>
        public int PermissionLevel { get; set; } = 1;

        /// <summary>
        /// Date when the relationship was established (e.g., enrollment date)
        /// </summary>
        public DateTime? LinkDate { get; set; }

        /// <summary>
        /// Audit: Who created this relationship
        /// </summary>
        [StringLength(256)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Audit: When this relationship was created
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// Audit: Who last updated this relationship
        /// </summary>
        [StringLength(256)]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Audit: When this relationship was last updated
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Current state (1=Active, 0=Inactive/Revoked)
        /// </summary>
        public int CurrentState { get; set; } = 1;
    }
}
