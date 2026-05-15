using LMSProject.Areas.Admin.Helpers;
using System.ComponentModel.DataAnnotations;

namespace LMSProject.ViewModel
{
    public class StudentVM
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string ParentMobile { get; set; }
        [Required]
        public string ParentNationalId { get; set; }
      
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string? ReturnUrl { get; set; }
        public string? ImageName { get; set; } = null!;
        public IFormFile? Image { get; set; }
        public string? CreatedBy { get; set; } = "User";
        public int GradeId { get; set; }
        
        public List<SelectDropList>? Grades { get; set; }
        

    }
}

