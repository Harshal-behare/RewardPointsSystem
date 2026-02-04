using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Reflection;
using FluentValidation.AspNetCore;
using RewardPointsSystem.Application.Configuration;
using RewardPointsSystem.Application;
using RewardPointsSystem.Infrastructure;
using RewardPointsSystem.Infrastructure.Data;

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
            // 2. CLEAN ARCHITECTURE - DEPENDENCY INJECTION
            // =====================================================
            // Infrastructure layer (DbContext, Repositories, External Services)
            builder.Services.AddInfrastructure(configuration);
            
            // Application layer (Business Services, AutoMapper, FluentValidation)
            builder.Services.AddApplication();

            // HTTP Context Accessor for ICurrentUserContext
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<RewardPointsSystem.Application.Interfaces.ICurrentUserContext, 
                RewardPointsSystem.Api.Services.HttpCurrentUserContext>();

            // =====================================================
            // 3. FLUENTVALIDATION ASP.NET CORE INTEGRATION
            // =====================================================
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddFluentValidationClientsideAdapters();

            // =====================================================
            // 4. HEALTH CHECKS (MVP)
            // =====================================================
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<RewardPointsDbContext>("database");

            // =====================================================
            // 5. CONTROLLERS & API EXPLORER
            // =====================================================
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // =====================================================
            // 6. SWAGGER/OPENAPI CONFIGURATION
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
            // 7. JWT AUTHENTICATION
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
            // 8. AUTHORIZATION POLICIES
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

            // Global Exception Handler (MVP - catches unhandled exceptions)
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";

                    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (exceptionHandlerFeature != null)
                    {
                        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError(exceptionHandlerFeature.Error, "Unhandled exception occurred");

                        await context.Response.WriteAsJsonAsync(new
                        {
                            success = false,
                            message = "An unexpected error occurred. Please try again.",
                            timestamp = DateTime.UtcNow
                        });
                    }
                });
            });

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

            // Health Check Endpoint (MVP)
            app.MapHealthChecks("/health");

             //=====================================================
             //12.DATABASE SEEDING
             //=====================================================
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
