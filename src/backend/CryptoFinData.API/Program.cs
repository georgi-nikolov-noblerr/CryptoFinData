using System.Text;
using System.Text.Json;
using CryptoFinData.Core.Entities;
using CryptoFinData.Core.Interfaces.External;
using CryptoFinData.Core.Interfaces.Services;
using CryptoFinData.Core.Options;
using CryptoFinData.Infrastructure.External;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using Refit;
using Serilog;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/cryptofindata-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CryptoFinData API",
                Version = "v1",
                Description = "API for cryptocurrency data"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CryptoFinDataPolicy", policy =>
            {
                policy
                    .WithOrigins(
                        builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ??
                        ["http://localhost:3242"]
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

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
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ??
                                               throw new InvalidOperationException("JWT Key is not configured")))
                };
            });

        builder.Services.Configure<CoinDeskApiOptions>(
            builder.Configuration.GetSection(CoinDeskApiOptions.SectionName));

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("CryptoFinData.Infrastructure")
            ));

        var refitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
        };

        builder.Services
            .AddRefitClient<ICoinDeskApi>(refitSettings)
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress =
                    new Uri(builder.Configuration["CoinDeskApi:BaseUrl"] ?? "https://api.coindesk.com/v1/bpi");
                c.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        builder.Services.AddScoped<ICryptoApiClient, CoinDeskApiClient>();
        builder.Services.AddScoped<ICryptoPriceService, CryptoPriceService>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<GzipCompressionProvider>();
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
        }
        
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "CryptoFinData API V1");
            c.RoutePrefix = "swagger";
        });

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseHttpsRedirection();
        app.UseResponseCompression();

        app.UseCors("CryptoFinDataPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
            .WithName("healthCheck")
            .WithOpenApi();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<AppDbContext>();

                if (app.Environment.IsDevelopment())
                {
                    context.Database.EnsureCreated();
                }

                context.Database.Migrate();

                if (!context.Users.Any())
                {
                    context.Users.Add(new User("admin@cryptofindata.com", "admin", BCrypt.Net.BCrypt.HashPassword("admin")));
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating or seeding the database.");
                throw;
            }
        }

        try
        {
            Log.Information("Starting CryptoFinData API");
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "API terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }

        return;

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        Log.Warning(
                            exception.Exception,
                            "Request failed with {Message}. Retry {RetryCount} after {TimeSpan}",
                            exception.Exception.Message,
                            retryCount,
                            timeSpan);
                    }
                );
        }

        static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    5,
                    TimeSpan.FromSeconds(30),
                    onBreak: (exception, duration) =>
                    {
                        Log.Warning("Circuit breaker opened for {Duration}", duration);
                    },
                    onReset: () => { Log.Information("Circuit breaker reset"); }
                );
        }
    }
}
