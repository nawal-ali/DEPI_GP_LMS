using System.Collections.Generic;

namespace LMSProject.ViewModel
{
    /// <summary>
    /// ViewModel for Assignments page
    /// </summary>
    public class AssignmentsViewModel
    {
        public List<Assignment> Assignments { get; set; } = new List<Assignment>();
        public string SearchTerm { get; set; }
        public string SelectedCourse { get; set; }
        public string SelectedTerm { get; set; }
        public string SelectedStatus { get; set; }

        // For filter dropdowns
        public List<string> Courses { get; set; } = new List<string>();
        public List<string> Terms { get; set; } = new List<string>();
        public List<string> Statuses { get; set; } = new List<string> { "Active", "Ended", "Draft" };
    }
}
