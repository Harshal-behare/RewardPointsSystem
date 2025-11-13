using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Reflection;
using RewardPointsSystem.Api.Configuration;
using RewardPointsSystem.Infrastructure.Data;

namespace RewardPointsSystem.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =====================================================
            // 1. CONFIGURATION
            // =====================================================
            var configuration = builder.Configuration;

            // =====================================================
            // 2. DATABASE CONFIGURATION
            // =====================================================
            builder.Services.AddDbContext<RewardPointsDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // =====================================================
            // 3. DEPENDENCY INJECTION - SERVICES
            // =====================================================
            builder.Services.RegisterRewardPointsServices(configuration);

            // =====================================================
            // 4. CONTROLLERS & API EXPLORER
            // =====================================================
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // =====================================================
            // 5. SWAGGER/OPENAPI CONFIGURATION
            // =====================================================
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Reward Points System API",
                    Description = "Production-grade reward points management API with JWT authentication, built with Clean Architecture principles",
                    Contact = new OpenApiContact
                    {
                        Name = "Development Team",
                        Email = "dev@rewardpoints.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });

                // JWT Authentication in Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                                  "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                                  "Example: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

                // XML Documentation
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            // =====================================================
            // 6. JWT AUTHENTICATION (Future Phase 7)
            // =====================================================
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

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
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero // Immediate token expiration
                };
            });

            builder.Services.AddAuthorization();

            // =====================================================
            // 7. CORS CONFIGURATION
            // =====================================================
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            // =====================================================
            // 8. BUILD THE APPLICATION
            // =====================================================
            var app = builder.Build();

            // =====================================================
            // 9. MIDDLEWARE PIPELINE
            // =====================================================
            
            // Swagger (Development and Production for testing)
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Reward Points API v1");
                options.RoutePrefix = string.Empty; // Swagger UI at root URL
                options.DocumentTitle = "Reward Points API Documentation";
            });

            // HTTPS Redirection
            app.UseHttpsRedirection();

            // CORS
            app.UseCors("AllowSpecificOrigins");

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map Controllers
            app.MapControllers();

            // =====================================================
            // 10. RUN THE APPLICATION
            // =====================================================
            Console.WriteLine("========================================");
            Console.WriteLine("  Reward Points System API");
            Console.WriteLine("========================================");
            Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
            Console.WriteLine($"Swagger UI: http://localhost:5000 (or configured port)");
            Console.WriteLine("========================================\n");

            app.Run();
        }
    }
}
