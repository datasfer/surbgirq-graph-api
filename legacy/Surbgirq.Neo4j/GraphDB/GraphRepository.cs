using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using Neo4jMapper;
using Serilog;
using ServiceStack;
using Surbgirq.GraphApi.Configs;
using Surbgirq.GraphApi.Extensions;
using Surbgirq.GraphApi.Interfaces;
using Surbgirq.GraphApi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace Surbgirq.GraphApi.GraphDB
{
    public class GraphRepository : IGraphRepository, IDisposable
    {
        private readonly IDriver _driver;
        //private readonly ICypherBuilderFactory _cypherBuilderFactory;

        private IAsyncSession AsyncSession() => _driver.AsyncSession();
        private readonly Neo4jConfig _config;
        private readonly ILogger<GraphRepository> _logger;
        private readonly ICypherBuilderFactory _cypherBuilderFactory;
        public GraphRepository( IDriver driver,
                                Neo4jConfig config,
                                ICypherBuilderFactory cypherBuilderFactory,
                                ILogger<GraphRepository> logger) 
        {
            _driver = driver;
            _config = config;
            _logger = logger;
            _cypherBuilderFactory = cypherBuilderFactory;
        }

        public async Task<IEnumerable<Entity<TNode>>> GetAllEntities<TNode>((string, string) s, IEnumerable<string> relationships) where TNode : class
        {
            return null;
        }

        public async Task<IEnumerable<Entity<TNode>>> GetAllEntities<TNode>()
           where TNode : class
        {
            IEnumerable<Entity<TNode>> result = new List<Entity<TNode>>();
            using var dbSession = DbSession.StartSession(_driver, _config.DatabaseName);
            try
            {
                string cypher = CypherQuery.GetAll<TNode>();
                var cursor = await dbSession.AsyncSession.RunAsync(cypher);
                result = (await cursor?.ToListAsync()).Select(process);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Filed to get {typeof(TNode).Name}");
            }

            return result;

            Entity<TNode> process(IRecord _) => new Entity<TNode>
            {
                Node = _[0].As<INode>().Properties.AsJson().AsPoco<TNode>(),
                Relationships = Transform(_[1].As<IPath>())
            };
        }
           
        /// <summary>
        ///     Find all nodes with label corresponding to the T type.
        /// </summary>
        /// <typeparam name="T">Type of the Model that will be searched in the database.</typeparam>
        public async Task<IEnumerable<T>> GetNodesByLabel<T>() where T : BaseModel
        {
            var dbSession = DbSession.StartSession(_driver, _config.DatabaseName);
            var query = CypherQuery.GetAll<T>();
            var cursor = await dbSession.AsyncSession.RunAsync(query);
            return (await cursor.ToListAsync()).Map<T>();
        }


        public async Task<IEnumerable<string>> GetNodesJsonOf(string nodeName)
        {
            var dbSession = DbSession.StartSession(_driver, _config.DatabaseName);
            var query = CypherQuery.GetAll(nodeName);
            var cursor = await dbSession.AsyncSession.RunAsync(query);
            return (await cursor.ToListAsync()).Select(process);

            string process(IRecord _) => _[0].As<INode>().Properties.AsJson();
        }

        public async Task<IEnumerable<IDictionary<string, object>>> RunQuery(string query)
        {
            var dbSession = DbSession.StartSession(_driver, _config.DatabaseName);
            var cursor = await dbSession.AsyncSession.RunAsync(query);
            return (await cursor.ToListAsync()).Select(process);

            IDictionary<string, object> process(IRecord record) 
            {
                var vs = record.Values;
                var res = new Dictionary<string, object>();
                foreach (var v in vs)
                {

                    switch (v.Value)
                    {
                        case INode node:
                            foreach (var np in node.Properties)
                            {
                                res.Add(np.Key, np.Value);
                            }
                            break;
                        case IPath path:
                            // TODO: process this case
                            res.Add(v.Key, path);
                            break;
                        default:
                            res.Add($"{v.Key}", v.Value);
                            break;
                    }
                }

                return res;
            }
            //_[0])](_[0]);
            
        }


        /// <summary>
        ///     Find single node with label corresponding to the T type and parameters passed.
        /// </summary>
        /// <param name="parameter">Tuple (parameter name, parameter value) which will be used in the search.</param>
        /// <typeparam name="T">Type of the Model that will be searched in the database.</typeparam>
        public async Task<T> First<T>((string, string) parameter) where T : BaseModel
        {
            var modelName = typeof(T).Name;
            var (key, value) = parameter;
            using var dbSession = DbSession.StartSession(_driver, _config.DatabaseName);

            var query = new StringBuilder($"MATCH (x:{modelName}");
            query.AppendLine($" {{{key}: '{value}'}})");
            query.Append("RETURN x");

            var queryContent = query.ToString();
            _logger.LogInformation(queryContent);
            
            var cursor = await dbSession.AsyncSession.RunAsync(queryContent);
            var result = (await cursor.ToListAsync()).Map<T>().FirstOrDefault();
            return result;
        }


        /// <summary>
        ///     Find all nodes with label corresponding to the T type and parameters passed.
        /// </summary>
        /// <param name="parameter">Tuple (parameter name, parameter value) which will be used in the search.</param>
        /// <typeparam name="T">Type of the Model that will be searched in the database.</typeparam>
        public async Task<IEnumerable<T>> FindAll<T>((string, string) parameter) where T : BaseModel
        {
            using var dbSession = DbSession.StartSession(_driver, _config.DatabaseName);

            var queryContent = CypherQuery.Find<Company>(parameter);
            _logger.LogInformation(queryContent);
            
            try
            {
                var cursor = await dbSession.AsyncSession.RunAsync(queryContent);
                var result = (await cursor.ToListAsync()).Map<T>();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed find all");   
                return null;
            }
        }


        #region private

        private IEnumerable<IRelationship> Transform(IPath path)
        {
            IDictionary<long, INode> nodes = path.Nodes.Distinct().ToDictionary(_ => _.Id);

            return path.Relationships.Select(_ => new Relationship { One = nodes[_.StartNodeId].Properties, Two = nodes[_.EndNodeId].Properties, Type = _.Type });
        }

       
        private string GetAllCypher<TNode>((string, string) keyValue, IEnumerable<string> relationships)
           where TNode : class
        {
            string node = typeof(TNode).Name.ToLowerInvariant();
            return _cypherBuilderFactory.NewCypherBuilder()
                .AddMatch(new[] { new Match { Pattern = $"(e:{node})" } })
                .AddWhere($"e.{keyValue.Item1} = '{keyValue.Item2}'")
                .AddMatch(new[] { new MatchWithAlias { Alias = "path", Pattern = $"(e) <-[{string.Join('|', relationships.Select(_ => $":{_}"))} *]-()" } })
                .AddReturn(new[] { "e, path" })
                .ToString();
        }

        private async Task<List<IRecord>> RunCypherWithResults(IAsyncTransaction tx, string cypher, Dictionary<string, object> parameters = null)
        {
            IResultCursor result;

            if (parameters != null)
            {
                result = await tx.RunAsync(cypher, parameters);
            }
            else
            {
                result = await tx.RunAsync(cypher);
            }


            //await result.ForEachAsync(record =>
            //{

            //    var node = record["e"].As<INode>();
            //    var chapter = new Chapter
            //    {
            //        Book = node[" book"].As<string>(),
            //        Number = node[" num"].As<int>(),
            //    };

            //    Debug.WriteLine(chapter.ToString());
            //});


            return await result?.ToListAsync();
        }

        #endregion

        public void Dispose()
        {
            _driver?.Dispose();
            GC.SuppressFinalize(this);
        }

    }
}
