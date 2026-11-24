#nullable disable warnings

namespace HolyGraph.API.Configs
{
    public class Neo4jConfig
    {
        public string InstanceUri { get; set; }
        public string DatabaseName { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }

        /// <summary>
        /// The settings that are missing or still a placeholder (empty, or CHANGE_ME), by the names of the environment
        /// variables that set them. The values themselves are never part of the result.
        /// </summary>
        public List<string> MissingSettings()
        {
            var settings = new (string Name, string Value)[]
            {
                ("Neo4jConfig__InstanceUri", InstanceUri),
                ("Neo4jConfig__UserName", UserName),
                ("Neo4jConfig__Password", Password),
                ("Neo4jConfig__DatabaseName", DatabaseName),
            };
            return settings
                .Where(s => string.IsNullOrWhiteSpace(s.Value) || s.Value.Trim() == "CHANGE_ME")
                .Select(s => s.Name)
                .ToList();
        }
    }
}
