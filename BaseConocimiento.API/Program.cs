using BaseConocimiento.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
var builder = WebApplication.CreateBuilder(args);

builder.Services.ValidateConfiguration(builder.Configuration);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"))
        ),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.Load("BaseConocimiento.Application"));
});

builder.Services.AddControllers();

// CORS para React/Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://192.168.0.246:3000",
                "http://localhost:3000",
                "http://192.168.0.246:3000",
                "https://inuzaru.taild221a4.ts.net",
                "http://inuzaru.taild221a4.ts.net",
                "http://localhost:5173",
                "http://localhost:5174"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Content-Disposition");
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Base de Conocimiento API",
        Version = "v1",
        Description = "API REST para gestión de base de conocimiento con RAG (Retrieval Augmented Generation)",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Equipo de Desarrollo",
            Email = "dev@empresa.com"
        }
    });

    options.MapType<IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600;
});


var qdrantHost = builder.Configuration["Qdrant:Host"];
var qdrantPort = builder.Configuration["Qdrant:RestPort"] ?? "6333";

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("PostgreSQL"), name: "PostgreSQL")
    .AddRedis(builder.Configuration["Redis:ConnectionString"], name: "Redis")
    .AddUrlGroup(
        new Uri($"http://{qdrantHost}:{qdrantPort}/metrics"),
        name: "Qdrant"
    )
    .AddUrlGroup(
        new Uri($"{builder.Configuration["AI:OllamaBaseUrl"]}/api/tags"),
        name: "Ollama"
    );

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Base de Conocimiento API v1");
        //options.RoutePrefix = string.Empty; // Swagger en la raíz (http://localhost:5000)
    });

    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Endpoint de salud
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            servicios = report.Entries.Select(e => new
            {
                nombre = e.Key,
                estado = e.Value.Status.ToString(),
                duracion = e.Value.Duration.TotalMilliseconds + "ms"
            })
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

Console.WriteLine("?? Base de Conocimiento API");
Console.WriteLine($"? Entorno: {app.Environment.EnvironmentName}");

app.Run();