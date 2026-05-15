namespace MLSCore.Models
{
    public class TbStudentTest
    {public int Id { get; set; }
        public DateTime JoinDate {  get; set; }
        public TimeOnly JoinTime {  get; set; }
        public int TimeInMinutes {  get; set; }
        public double Score {  get; set; }
        [ForeignKey("Student")]
        public int StudentId {  get; set; }
        public TbStudent Student { get; set; }
        [ForeignKey("Test")]
        public int TestId {  get; set; }
        public TbTest Test { get; set; }
        public List<TbStudentAnswer> Answers {  get; set; }
    }
}
