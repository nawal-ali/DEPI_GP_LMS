namespace LMSProject.Areas.Admin.ViewModel
{
    public class TermVM
    {
        public string Name { get; set; }

        public string? CreatedBy { get; set; } = "Admin";


        public string? ImageName { get; set; } = null!;

        public bool ShowInHomePage { get; set; }
        public IFormFile? Image { get; set; }

    }
    public class TermEditVM
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string? UpdatedBy { get; set; } = "Admin";


        public string? ImageName { get; set; } = null!;

        public bool ShowInHomePage { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public IFormFile? Image { get; set; }
    }
}
