using System;
using System.Collections.Generic;
using System.Text;

namespace MLSCore.Models
{
    public class TbCourseGroup
    {
        public int Id { get; set; }

        public string GroupName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int Capacity { get; set; }
        public int CurrentState { get; set; } = 1;
        // FK
        [ForeignKey("Course")]
        public int CourseId { get; set; }

        public TbCourse Course { get; set; }

        
    }
}
