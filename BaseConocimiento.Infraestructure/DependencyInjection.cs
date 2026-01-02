using BaseConocimiento.Application.Interfaces.AI;
using BaseConocimiento.Application.Interfaces.Auth;
using BaseConocimiento.Application.Interfaces.Conversation;
using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Application.Interfaces.Processing;
using BaseConocimiento.Application.Interfaces.Storage;
using BaseConocimiento.Application.Interfaces.VectorStore;
using BaseConocimiento.Infrastructure.Data;
using BaseConocimiento.Infrastructure.Repositories;
using BaseConocimiento.Infrastructure.Services.AI.Gemini;
using BaseConocimiento.Infrastructure.Services.AI.Ollama;
using BaseConocimiento.Infrastructure.Services.Auth;
using BaseConocimiento.Infrastructure.Services.Conversation;
using BaseConocimiento.Infrastructure.Services.Processing;
using BaseConocimiento.Infrastructure.Services.Storage;
using BaseConocimiento.Infrastructure.Services.VectorStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        //POSTGRESQL
        services.AddDbContext<BaseConocimientoDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("PostgreSQL"),
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                    npgsqlOptions.CommandTimeout(30);
                }
            );
        });

        //UNIT OF WORK
        services.AddScoped<IManualRepository, ManualRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        //FILE STORAGE
        services.AddScoped<IFileStorageService, FileStorageService>();

        //VECTOR STORE (QDRANT)
        services.AddHostedService<QdrantInitializerHostedService>();
        services.AddSingleton<IQdrantService, QdrantService>();

        //REDIS
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:ConnectionString"] ?? "localhost:6379";
            options.InstanceName = "BaseConocimiento:";
        });

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configurationOptions = ConfigurationOptions.Parse(
                configuration["Redis:ConnectionString"] ?? "localhost:6379"
            );
            configurationOptions.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        services.AddScoped<IConversationService, RedisConversationService>();

        //PDF PROCESSING
        services.AddScoped<IPdfProcessingService, PdfProcessingService>();
        services.AddScoped<ITextProcessingService, TextProcessingService>();

        //AI SERVICES
        var aiProvider = configuration["AI:Provider"]?.ToLower() ?? "gemini";

        switch (aiProvider)
        {
            case "gemini":
                services.AddHttpClient<GeminiEmbeddingService>();
                services.AddHttpClient<GeminiChatCompletionService>();
                services.AddScoped<IEmbeddingService, GeminiEmbeddingService>();
                services.AddScoped<IChatCompletionService, GeminiChatCompletionService>();
                break;

            case "ollama":
                var ollamaUrl = configuration["AI:OllamaBaseUrl"];

                services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>(client =>
                {
                    client.BaseAddress = new Uri(ollamaUrl);
              
                });

                services.AddHttpClient<IChatCompletionService, OllamaChatService>(client =>
                {
                    client.BaseAddress = new Uri(ollamaUrl);
                });

                Console.WriteLine($"✓ Usando proveedor de IA Local (Vía Túnel): {ollamaUrl}");
                break;

            default:
                throw new InvalidOperationException(
                    $"Proveedor de IA no soportado: {aiProvider}. Use 'gemini' o 'ollama'.");
        }

        //LOGIN
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();


        return services;
    }

    public static void ValidateConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var errors = new List<string>();
        var aiProvider = configuration["AI:Provider"]?.ToLower();

        if (string.IsNullOrEmpty(configuration.GetConnectionString("PostgreSQL")))
            errors.Add("Falta: ConnectionStrings:PostgreSQL");

        if (string.IsNullOrEmpty(configuration["Qdrant:Host"]))
            errors.Add("Falta: Qdrant:Host");

        if (string.IsNullOrEmpty(aiProvider))
        {
            errors.Add("Falta: AI:Provider");
        }
        else if (aiProvider == "gemini")
        {
            // Gemini requiere API Key
            if (string.IsNullOrEmpty(configuration["AI:ApiKey"]))
                errors.Add("Falta: AI:ApiKey (Requerido para Gemini)");
        }
        // Ollama no requiere ApiKey

        if (errors.Any())
            throw new InvalidOperationException($"Configuración inválida:\n{string.Join("\n", errors)}");

        Console.WriteLine("✓ Configuración validada correctamente");
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BaseConocimientoDbContext>();
        try
        {
            await context.Database.MigrateAsync();
            Console.WriteLine("✓ Base de datos inicializada correctamente");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error al inicializar base de datos: {ex.Message}");
            throw;
        }
    }
}