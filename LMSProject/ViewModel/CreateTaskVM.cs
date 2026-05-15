using System;
using System.ComponentModel.DataAnnotations;

namespace LMSProject.ViewModel
{
    /// <summary>
    /// ViewModel for creating and editing tasks by instructors
    /// </summary>
    public class CreateTaskVM
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        [Display(Name = "Task Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Task URL/Link is required")]
        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Task URL or External Link")]
        public string URL { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Please select a sub-content")]
        [Display(Name = "Sub-Content")]
        public int SubContentId { get; set; }
    }
}
