using System.Collections.Generic;

namespace BlazorMultytenantDemo.Data
{
    public class Org
    {
        public int Id { get; set; }
        public string AdminName { get; set; } // Note: bad design! Fix when time allows.
        public ICollection<Relation> Relations { get; set; }
    }
}