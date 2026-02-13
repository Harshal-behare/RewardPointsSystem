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
using Serilog;

namespace RewardPointsSystem.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Bootstrap logger - write to console only to avoid file permission issues during tests
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting Reward Points System API...");

                var builder = WebApplication.CreateBuilder(args);

               
                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext());

               
                var configuration = builder.Configuration;

          
            builder.Services.AddInfrastructure(configuration);
            
           
            builder.Services.AddApplication();

           
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<RewardPointsSystem.Application.Interfaces.ICurrentUserContext, 
                RewardPointsSystem.Api.Services.HttpCurrentUserContext>();

            
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddFluentValidationClientsideAdapters();

           
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<RewardPointsDbContext>("database");

         
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

           
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
                    //    Email = "dev@agdata.com"
                    //},
                    //License = new OpenApiLicense
                    //{
                    //    Name = "MIT License",
                    //    Url = new Uri("https://agdata.com")
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

               
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

           
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

         
            builder.Services.AddAuthorization(options =>
            {
                // Admin-only policy
                options.AddPolicy("AdminOnly", policy => 
                    policy.RequireRole("Admin"));

                // Employee or Admin policy
                options.AddPolicy("EmployeeOrAdmin", policy => 
                    policy.RequireRole("Admin", "Employee"));

               
                options.AddPolicy("RequireAuthenticatedUser", policy => 
                    policy.RequireAuthenticatedUser());
            });

           
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

          
            var app = builder.Build();

            // =====================================================
            // 11. MIDDLEWARE PIPELINE
            // =====================================================

            // Global Exception Handler (MVP - catches unhandled exceptions)
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (exceptionHandlerFeature != null)
                    {
                        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                        var exception = exceptionHandlerFeature.Error;
                        
                        // Determine status code and message based on exception type
                        int statusCode = 500;
                        string message = "An unexpected error occurred. Please try again.";
                        
                        // Domain exceptions should return 400 with actual message
                        if (exception is RewardPointsSystem.Domain.Exceptions.DomainException domainEx)
                        {
                            statusCode = 400;
                            message = domainEx.Message;
                            logger.LogWarning(exception, "Domain exception: {Message}", message);
                        }
                        else
                        {
                            logger.LogError(exception, "Unhandled exception occurred");
                        }
                        
                        context.Response.StatusCode = statusCode;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsJsonAsync(new
                        {
                            success = false,
                            message = message,
                            timestamp = DateTime.UtcNow
                        });
                    }
                });
            });

          
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Reward Points API v1");
                options.RoutePrefix = string.Empty; 
                options.DocumentTitle = "Reward Points API Documentation";
            });

           
            // SERILOG REQUEST LOGGING (logs all HTTP requests)
           
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            });

            // HTTPS Redirection
            app.UseHttpsRedirection();

            // CORS
            app.UseCors("AllowSpecificOrigins");

           
            app.UseAuthentication();
            app.UseAuthorization();

            // Map Controllers
            app.MapControllers();

       
            app.MapHealthChecks("/health");

             
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

            
            Log.Information("========================================");
            Log.Information("  Reward Points System API Started");
            Log.Information("========================================");
            Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
            Log.Information("Swagger UI: http://localhost:1352");
            Log.Information("Logs are being written to: Logs/ folder");
            Log.Information("========================================");

            app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
