using HolyGraph.API.Configs;
using HolyGraph.API.Extensions;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(o => { });

builder.Services.AddControllers();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "My API - V1",
            Version = "v1"
        }
     );

    var filePath = Path.Combine(System.AppContext.BaseDirectory, "HolyGraph.API.xml");
    c.IncludeXmlComments(filePath);
});

builder.Services.AddApplicationServices(builder.Configuration);

// The Neo4j settings must be real values: the environment (Neo4jConfig__InstanceUri, __UserName, __Password,
// __DatabaseName) overrides the placeholders in appsettings.Development.json. Without them the API doesn't start.
var neo4jConfig = builder.Configuration.GetSection(nameof(Neo4jConfig)).Get<Neo4jConfig>() ?? new Neo4jConfig();
var missingSettings = neo4jConfig.MissingSettings();
if (missingSettings.Count > 0)
{
    Console.Error.WriteLine($"HolyGraph.API doesn't start: set {string.Join(", ", missingSettings)} " +
                            "(environment variables; see the README, \"Run the API\").");
    Environment.ExitCode = 1;
    return;
}

var app = builder.Build();

app.UseHttpLogging();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.UseCors(opt => opt.AllowAnyHeader().AllowAnyMethod().WithOrigins("*"));

// authentication authorization ges here

app.MapControllers();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
