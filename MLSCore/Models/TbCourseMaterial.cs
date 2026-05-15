using System.ComponentModel.DataAnnotations.Schema;

namespace MLSCore.Models
{
    public enum MaterialType { PDF, DOC, Image, Link }

    public class TbCourseMaterial
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string FileUrl { get; set; }
        public string? FileName { get; set; }
        public MaterialType MaterialType { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int CurrentState { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public TbCourse Course { get; set; }
    }
}
