using LMSProject.Areas.Admin.Helpers;

namespace LMSProject.Areas.Instructor.ViewModel
{
    public class CourseGroupVM
    {
        public string GroupName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int Capacity { get; set; }

        public int CourseId { get; set; }
        public List<SelectDropList>? Courses { get; set; }
    }
    public class CourseGroupEditVM:CourseGroupVM
    { public int Id { get; set; } }
}
