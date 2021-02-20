using System.Collections.Generic;

namespace BlazorMultytenantDemo.Data
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Relation> Relations { get; set; }

    }
}