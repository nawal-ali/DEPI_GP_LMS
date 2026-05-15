namespace MLSCore.Models
{
    public class TbStudentAnswer
    {
        public int Id { get; set; }
        public string TextAnswer { get; set; }//answerfor text question
        public double Score { get; set; }
        public bool Marked { get; set; }
        public int duration { get; set; }

        [ForeignKey("Question")]
        public int QuestionId { get; set; }
        public TbTestQuestion Question { get; set; }


        [ForeignKey("StudentTest")]
        public int StudentTestId { get; set; }
        public TbStudentTest StudentTest { get; set; }
        public List<TbSelectedChoice> SelectedChoices { get; set; }
    }
}
