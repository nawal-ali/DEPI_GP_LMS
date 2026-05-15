namespace LMSProject.ViewModel
{
    /// <summary>
    /// Simple model for Assignment display/listing
    /// </summary>
    public class Assignment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Course { get; set; }
        public string Term { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Deadline { get; set; }
        public string Status { get; set; }
        public int Submitted { get; set; }
        public int Total { get; set; }
    }
}
