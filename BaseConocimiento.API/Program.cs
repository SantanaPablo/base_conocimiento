// WebApi/Program.cs
using BaseConocimiento.Infrastructure;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ValidateConfiguration(builder.Configuration);

builder.Services.AddInfrastructure(builder.Configuration);

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

app.UseAuthorization();

app.MapControllers();

// Endpoint de salud
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0"
}));

Console.WriteLine("?? Base de Conocimiento API");
Console.WriteLine($"? Entorno: {app.Environment.EnvironmentName}");
Console.WriteLine($"? URL: {builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5000"}");
Console.WriteLine($"? Swagger: {(app.Environment.IsDevelopment() ? "http://localhost:5000" : "Deshabilitado")}");

app.Run();