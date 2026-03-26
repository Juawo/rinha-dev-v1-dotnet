using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
var builder = WebApplication.CreateBuilder(args);

// 1. Desative os logs para ganhar performance (conforme a dica da Rinha)
builder.Logging.ClearProviders();

// 2. Monte a String de Conexão usando as variáveis do Docker
var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var user = Environment.GetEnvironmentVariable("DB_USER") ?? "admin";
var pass = Environment.GetEnvironmentVariable("DB_PASS") ?? "123";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "rinha";

var connectionString = $"Host={host};Username={user};Password={pass};Database={dbName};Pooling=true;Minimum Pool Size=10;Maximum Pool Size=50";

builder.Services.AddNpgsqlDataSource(connectionString);

// builder.Logging.ClearProviders();

var app = builder.Build();


app.MapGet("/eventos", async (NpgsqlDataSource db) =>
{
    await using var connection = db.OpenConnection();
    await using var command = new NpgsqlCommand("SELECT * FROM eventos;", connection);
    await using var reader = await command.ExecuteReaderAsync();
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

app.MapPost("/reservas", async (ReservaRequest request, NpgsqlDataSource db) =>
{
    await using var connection = await db.OpenConnectionAsync();
    await using var command = new NpgsqlCommand(@"
        WITH atualizacao AS (
            UPDATE eventos 
            SET ingressos_disponiveis = ingressos_disponiveis - 1 
            WHERE id = @EventoId AND ingressos_disponiveis > 0 
            RETURNING id
        )
        INSERT INTO reservas (evento_id, usuario_id)
        SELECT id, @UsuarioId FROM atualizacao 
        RETURNING evento_id;
    ", connection);
    command.Parameters.AddWithValue("EventoId", request.evento_id);
    command.Parameters.AddWithValue("UsuarioId", request.usuario_id);
    var result = await command.ExecuteScalarAsync();
    return result == null ? Results.UnprocessableEntity() : Results.StatusCode(201);
});

app.Run("http://0.0.0.0:8080");


public record ReservaRequest(long evento_id, long usuario_id);
