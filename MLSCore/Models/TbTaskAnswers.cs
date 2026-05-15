namespace MLSCore.Models
{
    public class TbTaskAnswers
    {
        public int Id { get; set; }
        public double Score {  get; set; }
        public string AnswerURL { get; set; }
        public string Comment { get; set; }//comment from Instructor
         public string? CreatedBy { get; set; } = null!;
        public DateTime? CreatedDate { get; set; }
        public int CurrentState { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        [ForeignKey("SubContentTask")]
        public int TaskId { get; set; }
        public TbTask SubContentTask { get; set; }

        [ForeignKey("Student")]
        public int StudId { get; set; }
        public TbStudent Student { get; set; }

    }
}
