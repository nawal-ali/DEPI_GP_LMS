namespace MLSCore.Models
{
    public class TbSelectedChoice
    {public int Id { get; set; }

        [ForeignKey("Answer")]
        public int StAnswerId {  get; set; }
        public int Score {  get; set; }
        public TbStudentAnswer Answer { get; set; }

        [ForeignKey("Choice")]
        public int ChoiceId {  get; set; }
        public TbChoice Choice { get; set; }
    }
}
