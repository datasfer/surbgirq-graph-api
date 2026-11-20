using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surbgirq.GraphApi.GraphDB
{
    public class CypherQuery
    {
        public static string GetAll<TNode>() where TNode : class
        {
            var cypherBuilder = new CypherBuilder();
            var node = typeof(TNode).Name; 
            var query = cypherBuilder.AddMatch(new[] { new Match { Pattern = $"(n:{node})" } })
                                     .AddReturn(new[] { "n" });

            return query.ToString();
        }

        public static string GetAll(string node) 
        {
            var cypherBuilder = new CypherBuilder();
            var query = cypherBuilder.AddMatch(new[] { new Match { Pattern = $"(n:{node})" } })
                                     .AddReturn(new[] { "n" });

            return query.ToString();
        }


        public static string Find<TNode>((string, string) parameter) where TNode : class
        {

            var cypherBuilder = new CypherBuilder();
            var node = typeof(TNode).Name;
            var (key, value) = parameter;

            
            var query = cypherBuilder.AddMatch(new[] { new Match { Pattern = $"(n:{node})" } })
                                     .AddWhere($"n.{key} = '{value}'")
                                     .AddReturn(new[] { "n" });

            return query.ToString();
        }

        public static string Find1<TNode>((string, string) parameter) where TNode : class
        {
            var nodeName = typeof(TNode).Name;
            var (key, value) = parameter;

            var query = new StringBuilder($"MATCH (x:{nodeName}");
            query.AppendLine($" {{{key}: '{value}'}})");
            query.Append("RETURN x");

            return query.ToString();
        }
    }
}
