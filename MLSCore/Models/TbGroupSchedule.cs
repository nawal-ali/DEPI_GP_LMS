using System;
using System.Collections.Generic;
using System.Text;

namespace MLSCore.Models
{
    public class TbGroupSchedule
    {
        public int Id { get; set; }

        public DayOfWeek Day { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        
        [ForeignKey("Group")]
        public int GroupId { get; set; }

        public TbCourseGroup Group { get; set; }
    }
}
