using MLSCore.IdentityModel;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MLSCore.Models
{
    public class TbParent
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = "";

        [StringLength(50)]
        public string? Relationship { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(20)]
        public string? AlternativePhoneNumber { get; set; }

        [StringLength(256)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? NationalId { get; set; }

        [StringLength(255)]
        public string? ImageName { get; set; }

        public bool IsPrimaryGuardian { get; set; } = true;

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? Occupation { get; set; }

        public int CurrentState { get; set; } = 1;

        [StringLength(256)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(256)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = "";

        public ApplicationUser? User { get; set; }

        public ICollection<TbStudent> Children { get; set; } = new List<TbStudent>();
    }
}