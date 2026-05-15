namespace MLSCore.Models
{
    public enum QuestionType { Choice, Text,MultiChoice }
    public class TbTestQuestion
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Pints { get; set; }
        public string? CreatedBy { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }

        public int CurrentState { get; set; }
        
        public bool ShowInHomePage { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public QuestionType QuestionType { get; set; }
        [ForeignKey("Test")]
        public int TestId {  get; set; }
        public TbTest Test { get; set; }

        public List<TbChoice> Choices { get; set; }
    }
}
