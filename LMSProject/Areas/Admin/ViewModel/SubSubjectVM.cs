using LMSProject.Areas.Admin.Helpers;

namespace LMSProject.Areas.Admin.ViewModel
{
    public class SubSubjectIndexVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageName { get; set; }
        public int CurrentState {  get; set; }
        public string Subject { get; set; }
    }
    public class SubSubjectVM
    {

        public string Name { get; set; }
       public int SubjectId { get; set; }

        public string? CreatedBy { get; set; } = "Admin";


        public string? ImageName { get; set; } = null!;

        public bool ShowInHomePage { get; set; }
        public IFormFile? Image { get; set; }
        public List<SelectDropList>? Subjects { get; set; }
    }
    public class SubSubjectEditVM
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public string Name { get; set; }

        public string? UpdatedBy { get; set; } = "Admin";


        public string? ImageName { get; set; } = null!;

        public bool ShowInHomePage { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public IFormFile? Image { get; set; }
        public List<SelectDropList>? Subjects { get; set; }
    }
}

