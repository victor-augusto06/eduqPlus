using DotNetEnv;
using EduqPlus.API.Interfaces;
using EduqPlus.API.Models;
using EduqPlus.API.Service;
using EduqPlus.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.FileProviders;
using System.Text;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var contentRoot = builder.Environment.ContentRootPath;
var baseFilesDirectory = Path.GetFullPath(Path.Combine(contentRoot, "..", "..", "files"));

if (!Directory.Exists(baseFilesDirectory)) {
    Directory.CreateDirectory(baseFilesDirectory);
}

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");
var ollamaUrl = Environment.GetEnvironmentVariable("OLLAMA_URL");
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");

Console.WriteLine("=== DEBUG DE VARIAVEIS ===");
Console.WriteLine($"DB_CONNECTION: '{connectionString}'");
Console.WriteLine($"JWT_SECRET: '{jwtSecret}'");
Console.WriteLine("==========================");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new Exception("ERRO CRÍTICO: DB_CONNECTION não foi encontrada. Verifique se o arquivo .env existe e está na raiz do projeto.");

if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new Exception("ERRO CRÍTICO: JWT_SECRET não foi encontrada no arquivo .env.");

builder.Services.AddDbContext<EduqPlusContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 32))
    ));

builder.Services.AddKernel()
    .AddOllamaChatCompletion(
        endpoint: new Uri(ollamaUrl ?? "http://localhost:11434"),
        modelId: "llama3"
    )
    .AddOllamaEmbeddingGenerator(
        endpoint: new Uri(ollamaUrl ?? "http://localhost:11434"),
        modelId: "nomic-embed-text"
    );

builder.Services.AddControllers();

builder.Services.AddScoped<IIaService, IaService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IAvaliacaoService, AvaliacaoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ICursoService, CursoService>();
builder.Services.AddScoped<IDenunciaService, DenunciaService>();
builder.Services.AddScoped<IProdutorService, ProdutorService>();
builder.Services.AddScoped<IPromessaCursoService, PromessaCursoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(x => {
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x => {
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddCors(options => {
    options.AddPolicy("EduqPolicy", policy => {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT desta maneira: Bearer {seu_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("EduqPolicy");

app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(baseFilesDirectory),
    RequestPath = "/files"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();