using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surbgirq.GraphApi.GraphDB
{
    public class Entity<TNode>
           where TNode : class
    {
        public TNode Node { get; set; }
        
        public IEnumerable<IRelationship> Relationships { get; set; }

        public void AddRelationship(IRelationship relationship)
        {
            ((List<IRelationship>)(Relationships ??= new List<IRelationship>())).Add(relationship);
        }
    }
}
