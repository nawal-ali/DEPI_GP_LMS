using System.ComponentModel.DataAnnotations;

namespace LMSProject.Areas.Admin.ViewModel
{
    public class InstructorVM
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]      
        public string? Email {  get; set; }
        public string? PhoneNumber {  get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password {  get; set; }
        public string? ReturnUrl { get; set; }
        public string Bio { get; set; }
        public string Specialization { get; set; }
        public int ExperienceYears { get; set; }
        public bool ShowInHomePage { get; set; }
        public string? ImageName { get; set; } = null!;
        public IFormFile? Image { get; set; }
        public string? CreatedBy { get; set; } = "Admin";

    }
    public class InstructorEditVM
    {
      public int Id { get; set; }
        public string FullName { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        public string? ReturnUrl { get; set; }
        public string Bio { get; set; }
        public string Specialization { get; set; }
        public int ExperienceYears { get; set; }
        public bool ShowInHomePage { get; set; }
        public string? ImageName { get; set; } = null!;
        public IFormFile? Image { get; set; }
        public string? UpdatedBy { get; set; } = "Admin";

    }


}
