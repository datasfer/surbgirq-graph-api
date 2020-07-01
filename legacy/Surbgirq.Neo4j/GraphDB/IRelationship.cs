using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surbgirq.GraphApi.GraphDB
{
    public interface IRelationship
    {
        dynamic One { get; set; }
        dynamic Two { get; set; }
        string Type { get; set; }
    }
}
