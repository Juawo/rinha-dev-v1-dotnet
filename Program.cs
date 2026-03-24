using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

builder.Services.AddNpgsqlDataSource(
    builder.Configuration.GetConnectionString("DefaultConnection")
);

var app = builder.Build();

app.MapPost("/reservas", async (ReservaRequest request, NpgsqlDataSource db) =>
{
    await using var connection = db.OpenConnection();
    var command = new NpgsqlCommand(@"
        UPDATE eventos SET ingressos_disponiveis = ingressos_disponiveis - 1 
        WHERE id = @EventoId AND ingressos_disponiveis > 0 RETURNING ingressos_disponiveis;
   ", connection);
    command.Parameters.AddWithValue("EventoId", request.EventoId);
    var result = await command.ExecuteScalarAsync();
    return result == null ? Results.UnprocessableEntity() : Results.Created();
});

app.MapGet("/eventos", async (NpgsqlDataSource db) =>
{
    await using var connection = db.OpenConnection();
    var command = new NpgsqlCommand("SELECT * FROM eventos;", connection);
    var reader = await command.ExecuteReaderAsync();
    var eventos = new List<Object>();
    while (await reader.ReadAsync())
    {
        eventos.Add(new
        {
            id = reader.GetInt16(0),
            nome = reader.GetString(1),
            ingressos_disponiveis = reader.GetInt16(2)
            
        });
    }
    
    return Results.Ok(eventos);
});

app.Run("http://0.0.0.0:8080");


public record ReservaRequest(int EventoId, int UsuarioId);
