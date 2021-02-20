namespace BlazorMultytenantDemo.Data
{
    public class Relation   
    {
        public int OrgId { get; set; }
        public Org Org { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}