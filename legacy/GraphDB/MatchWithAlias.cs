using Surbgirq.GraphApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surbgirq.GraphApi.GraphDB
{
    public class MatchWithAlias : IMatch
    {
        public string Pattern { get; set; }
        public string Alias { get; set; }
    }
}
