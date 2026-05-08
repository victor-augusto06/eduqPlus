using DotNetEnv;
using EduqPlus.API.Interfaces;
using EduqPlus.API.Models;
using EduqPlus.API.Services;
using Microsoft.EntityFrameworkCore;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");

builder.Services.AddDbContext<EduqPlusContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

builder.Services.AddControllers();
builder.Services.AddScoped<ICursoService, CursoService>();

builder.Services.AddCors(options => {
    options.AddPolicy("EduqPolicy", policy => {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("EduqPolicy");
app.MapControllers();
app.Run();