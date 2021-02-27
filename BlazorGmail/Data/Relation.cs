namespace BlazorMultytenantDemo.Data
{
    public class Relation   
    {

        public User User { get; set; }
        public Org Org { get; set; }
        public int OrgId { get; set; }
        public int UserId { get; set; }
    }
}