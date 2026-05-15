namespace MLSCore.Models
{
    public class TbSessionAttend
    {
        public int Id { get; set; }
        public TimeOnly JoinAt {  get; set; }
        public TimeOnly LeftAt { get; set; }
        [ForeignKey("Student")]
        public int StudentId {  get; set; }
        public TbStudent Student { get; set; }
        [ForeignKey("SubContent")]
        public int SessionId {  get; set; }
        public TbSubContent SubContent { get; set; }
    }
}
