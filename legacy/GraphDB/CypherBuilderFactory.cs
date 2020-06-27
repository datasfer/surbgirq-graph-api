using Surbgirq.GraphApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surbgirq.GraphApi.GraphDB
{
    public class CypherBuilderFactory : ICypherBuilderFactory
    {
        public ICypherBuilder NewCypherBuilder()
        {
            return new CypherBuilder();
        }
    }
}
