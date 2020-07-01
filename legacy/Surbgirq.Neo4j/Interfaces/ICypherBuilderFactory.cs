using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surbgirq.GraphApi.Interfaces
{
    public interface ICypherBuilderFactory
    {
        ICypherBuilder NewCypherBuilder();
    }
}
