using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; 
using Ochakovo.Context;

namespace Ochakovo;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Настройка базы данных
        builder.Services.AddDbContext<OchakovoDbContext>(options =>
        {
            options.UseMySql(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")) 
            );
        });

        // 2. Настройка JSON (CamelCase и игнорирование циклических ссылок)
        builder.Services.AddControllers().AddJsonOptions(options => 
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles; 
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; 
        });

        // 3. ДОБАВЛЕНО: Настройка CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(
                        "https://localhost:7076", // Ваш текущий Origin из логов
                        "http://localhost:3000",  // Стандартный порт React/Next.js
                        "http://localhost:5173",  // Стандартный порт Vite/Vue
                        "http://localhost:4200",  // Стандартный порт Angular
                        "http://localhost:5174"   // Альтернативный порт Vite
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials(); // Обязательно для работы с JWT токенами/куки
            });
        });
       
        // 4. Настройка Swagger с поддержкой JWT
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ochakovo API", Version = "v1" });
            
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Введите JWT токен в формате: Bearer {ваш_токен}",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
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
        
        // 5. Настройка JWT Аутентификации
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key не найден в конфигурации");

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

        builder.Services.AddAuthorization();

        // === НАСТРОЙКА PIPELINE (ПОРЯДОК СТРОГО ВАЖЕН!) ===
        var app = builder.Build();
        
        if (app.Environment.IsDevelopment())
        {
            // ПОКАЖЕТ РЕАЛЬНУЮ ОШИБКУ (например, про Data truncated), а не 404
            app.UseDeveloperExceptionPage(); 
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            // В продакшене используем общую страницу ошибок
            app.UseExceptionHandler("/error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        
        // ВАЖНО: UseCors должен быть ДО UseAuthentication и UseAuthorization
        app.UseCors("AllowFrontend"); 
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}