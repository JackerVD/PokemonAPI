using Microsoft.EntityFrameworkCore;
using PokemonAPI.Data;
using PokemonAPI.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddSingleton<CombateService>();
builder.Services.AddSingleton<DanoService>();
builder.Services.AddOpenApi();

// DbContext con SQL Server
var connectionString = builder.Configuration.GetConnectionString("PokemonDB")
    ?? throw new InvalidOperationException("No se encontró la connection string 'PokemonDB'.");

builder.Services.AddDbContext<PokemonDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();