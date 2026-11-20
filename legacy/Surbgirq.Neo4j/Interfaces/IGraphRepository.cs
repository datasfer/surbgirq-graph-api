using Surbgirq.GraphApi.GraphDB;
using Surbgirq.GraphApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surbgirq.GraphApi.Interfaces
{
    public interface IGraphRepository
    {
        //Task<IEnumerable<Entity<TNode>>> GetCircularDependencies<TNode>(string relationship, (string, string) field)
        //   where TNode : class;
        Task<IEnumerable<Entity<TNode>>> GetAllEntities<TNode>()
            where TNode : class;

        Task<IEnumerable<Entity<TNode>>> GetAllEntities<TNode>((string, string) field, IEnumerable<string> relationships)
            where TNode : class;

        Task<IEnumerable<T>> GetNodesByLabel<T>() where T : BaseModel;
        Task<T> First<T>((string, string) parameter) where T : BaseModel;
        Task<IEnumerable<T>> FindAll<T>((string, string) parameter) where T : BaseModel;

        Task<IEnumerable<string>> GetNodesJsonOf(string nodeName);// where T : BaseModel;
        Task<IEnumerable<IDictionary<string, object>>> RunQuery(string query);

    }
}
