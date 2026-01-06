using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using RewardPointsSystem.Application.Configuration;
using RewardPointsSystem.Infrastructure.Data;
using RewardPointsSystem.Api.Configuration;

namespace RewardPointsSystem.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
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
            // 4. AUTOMAPPER CONFIGURATION (Phase 6)
            // =====================================================
            builder.Services.AddAutoMapper(typeof(RewardPointsSystem.Application.MappingProfiles.UserMappingProfile).Assembly);

            // =====================================================
            // 5. FLUENTVALIDATION CONFIGURATION (Phase 5)
            // =====================================================
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddFluentValidationClientsideAdapters();
            builder.Services.AddValidatorsFromAssemblyContaining<RewardPointsSystem.Application.Validators.Auth.LoginRequestDtoValidator>();

            // =====================================================
            // 6. CONTROLLERS & API EXPLORER
            // =====================================================
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // =====================================================
            // 7. SWAGGER/OPENAPI CONFIGURATION
            // =====================================================
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Reward Points System API",
                    Description = "Production-grade reward points management API with JWT authentication, built with Clean Architecture principles",
                    //Contact = new OpenApiContact
                    //{
                    //    Name = "Development Team",
                    //    Email = "dev@rewardpoints.com"
                    //},
                    //License = new OpenApiLicense
                    //{
                    //    Name = "MIT License",
                    //    Url = new Uri("https://opensource.org/licenses/MITz")
                    //}
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
            // 8. JWT AUTHENTICATION (Phase 7)
            // =====================================================
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            if (jwtSettings == null)
                throw new InvalidOperationException("JwtSettings section is missing in appsettings.json");
            
            jwtSettings.Validate(); // Validate JWT settings on startup
            builder.Services.AddSingleton(jwtSettings); // Register as singleton for dependency injection
            
            var secretKey = jwtSettings.SecretKey;

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
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero // Immediate token expiration
                };
            });

            // =====================================================
            // 8.1. AUTHORIZATION POLICIES
            // =====================================================
            builder.Services.AddAuthorization(options =>
            {
                // Admin-only policy
                options.AddPolicy("AdminOnly", policy => 
                    policy.RequireRole("Admin"));

                // Employee or Admin policy
                options.AddPolicy("EmployeeOrAdmin", policy => 
                    policy.RequireRole("Admin", "Employee"));

                // Require authenticated user
                options.AddPolicy("RequireAuthenticatedUser", policy => 
                    policy.RequireAuthenticatedUser());
            });

            // =====================================================
            // 9. CORS CONFIGURATION
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
            // 10. BUILD THE APPLICATION
            // =====================================================
            var app = builder.Build();

            // =====================================================
            // 11. MIDDLEWARE PIPELINE
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
            // 12. DATABASE SEEDING
            // =====================================================
            //using (var scope = app.Services.CreateScope())
            //{
            //    var services = scope.ServiceProvider;
            //    try
            //    {
            //        await DatabaseSeeder.SeedAsync(services);
            //    }
            //    catch (Exception ex)
            //    {
            //        var logger = services.GetRequiredService<ILogger<Program>>();
            //        logger.LogError(ex, "An error occurred while seeding the database");
            //    }
            //}

            // =====================================================
            // 13. RUN THE APPLICATION
            // =====================================================
            Console.WriteLine("========================================");
            Console.WriteLine("  Reward Points System API");
            Console.WriteLine("========================================");
            Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
            Console.WriteLine($"Swagger UI: http://localhost:1352 ");
            Console.WriteLine("========================================\n");

            app.Run();
        }
    }
}
