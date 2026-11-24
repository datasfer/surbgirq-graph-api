
namespace HolyGraph.API.Interfaces;

public interface INeo4jGraphRepository
{
    Task<string> RunQueryAsync(string query);
    Task<IEnumerable<T>> RunQueryAsync<T>(string query);

    /// <summary>
    /// Runs a query whose values come as parameters (never pasted into the text) and returns the first value of the
    /// first record, or null when there is none.
    /// </summary>
    Task<object?> RunScalarAsync(string query, object parameters);
}