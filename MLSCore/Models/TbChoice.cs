

namespace MLSCore.Models
{
    public class TbChoice
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool Correct {  get; set; }
        [ForeignKey("Question")]
        public int QuestionId { get; set; }
        public TbTestQuestion Question { get; set; }
        public List<TbSelectedChoice> SelectedChoices {  get; set; }
    }
}
