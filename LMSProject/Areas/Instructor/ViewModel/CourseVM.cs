using LMSProject.Areas.Admin.Helpers;
using MLSCore.Models;

namespace LMSProject.Areas.Instructor.ViewModel
{
    public class CourseVM
    {
        public string Name { get; set; }
        public CourseStatus Status { get; set; }
        public int TermId { get; set; }
        public int GradeId { get; set; }
        public int SubSubjId { get; set; }
        public int InstructorId { get; set; }

        public double Price { get; set; }
        public string? CreatedBy { get; set; } = "Admin";


        public string? ImageName { get; set; } = null!;

        public bool ShowInHomePage { get; set; }
        public IFormFile? Image { get; set; }
        public List<SelectDropList>? Terms { get; set; }
        public List<SelectDropList>? Grades { get; set; }
        public List<SelectDropList>? Subjects { get; set; }
        public List<SelectDropList>? SubSubjects { get; set; }
    }
    public class CourseEditVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CourseStatus Status { get; set; }
        public int TermId { get; set; }
        public int GradeId { get; set; }
        public int SubjId {  get; set; }
        public int SubSubjId { get; set; }     

        public double Price { get; set; }
        public string? UpdatedBy { get; set; } = "Admin";


        public string? ImageName { get; set; } = null!;

        public bool ShowInHomePage { get; set; }
        public IFormFile? Image { get; set; }
        public List<SelectDropList>? Terms { get; set; }
        public List<SelectDropList>? Grades { get; set; }
        public List<SelectDropList>? Subjects { get; set; }
        public List<SelectDropList>? SubSubjects { get; set; }
    }
}
