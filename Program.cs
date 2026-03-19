using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

builder.Services.AddNpgsqlDataSource(
    builder.Configuration.GetConnectionString("DefaultConnection")
);

var app = builder.Build();

app.MapGet("/ping", async (NpgsqlDataSource db) =>
{
    await using var conn = await db.OpenConnectionAsync();
    Console.WriteLine("Pong");
    return "Ok";
});

app.Run("http://0.0.0.0:8080");
