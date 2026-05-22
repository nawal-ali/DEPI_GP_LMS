using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MLSCore.Models
{
    /// <summary>
    /// Represents a recurring weekly session slot for a course.
    /// Multiple sessions per course (e.g. Mon 9-10, Wed 9-10).
    /// </summary>
    public class TbScheduleSession
    {
        public int Id { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public TbCourse Course { get; set; } = null!;

        /// <summary>0=Sunday,1=Monday,2=Tuesday,3=Wednesday,4=Thursday,5=Friday,6=Saturday</summary>
        [Required]
        public int DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [MaxLength(500)]
        public string? MeetingLink { get; set; }

        /// <summary>Hex color for the session card (e.g. #5B72EE)</summary>
        [MaxLength(20)]
        public string Color { get; set; } = "#5B72EE";

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int CurrentState { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}