namespace MLSCore.Models
{
    public enum CourseStatus { OnLine, Recorded }
    public class TbCourse
    {
        public int Id { get; set; }

        [Display(Name = "Course Name")]
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public CourseStatus status { get; set; }
        public double Rating { get; set; }
        public double Price { get; set; }
        public string? CreatedBy { get; set; } = null!;
        public DateTime? CreatedDate { get; set; }
        public int CurrentState { get; set; }
        public string ImageName { get; set; } = null!;
        public bool ShowInHomePage { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }


        [ForeignKey("Term")]
        public int TermId { get; set; }
        public TbTerm Term { get; set; }


        [ForeignKey("Grade")]
        public int GradeId { get; set; }
        public TbGrade Grade { get; set; }


        [ForeignKey("SubSubject")]
        public int SubSubjId { get; set; }
        public TbSubSubject SubSubject { get; set; }


        [ForeignKey("Instructor")]
        public int InstructorId { get; set; }
        public TbInstructor Instructor { get; set; }

        // ── Live course fields ──────────────────────────────────
        /// <summary>True = Live sessions, False = Recorded</summary>
        public bool IsLive { get; set; } = false;
        public int? NumberOfSessions { get; set; }
        public int? SessionDurationMinutes { get; set; }
        [MaxLength(500)]
        public string? MeetingLink { get; set; }
        public DateTime? LiveStartDate { get; set; }
        public DateTime? LiveEndDate { get; set; }

        public List<TbScheduleSession>? ScheduleSessions { get; set; }

        public List<TbCourseReview> CourseReviews { get; set; }
        public List<TbCourseDiscount> Discounts { get; set; }
        public List<TbCourseContent> CourseContents { get; set; }
        public List<TbCourseGroup> CourseGroups { get; set; }
        public List<TbCourseMaterial>? CourseMaterials { get; set; }
        public List<TbAssignment>? Assignments { get; set; }
        public List<TbStudentCourse>? StudentCourses { get; set; }

    }
}