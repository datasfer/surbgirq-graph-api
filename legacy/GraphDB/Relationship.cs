using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Surbgirq.GraphApi.GraphDB
{
    public class Relationship : IRelationship
    {
        public dynamic One { get; set; }
        public dynamic Two { get; set; }
        public string Type { get; set; }
    }
}
