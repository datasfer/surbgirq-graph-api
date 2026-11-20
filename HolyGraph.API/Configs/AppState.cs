using HolyGraph.API.Yaml;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HolyGraph.API;

public class AppState
{
    // Book metadata (chapter and verse counts, abbreviations) of the 1994 Etchmiadzin translation.
    // The files are copyrighted, so they are not in this repository (see README, "Data"). Put them in
    // HolyGraph.API/Data/YAML/e1994T/New (git-ignored); the build copies them next to the app.
    // Without them the API still starts, and uses default chapter and verse counts.
    private const string MetadataFolder = "Data/YAML/e1994T/New";

    public AppState()
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(new CamelCaseNamingConvention())
            .Build();

        var folder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, MetadataFolder));
        Books = [];
        if (Directory.Exists(folder))
        {
            foreach (var file in Directory.GetFiles(folder, "*.yaml").Order(StringComparer.Ordinal))
            {
                using var reader = File.OpenText(file);
                Books.Add(deserializer.Deserialize<Book>(reader));
            }
        }

        Console.WriteLine(Books.Count > 0
            ? $"Book metadata: {Books.Count} books from {folder}"
            : $"No book metadata in {folder}; using default chapter and verse counts.");
    }

    public List<Book> Books {get; private set;}
}
