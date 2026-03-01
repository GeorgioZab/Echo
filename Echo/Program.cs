using Echo.Api.Services;
using Echo.Application.Interfaces;
using Echo.Application.Users.Commands;
using Echo.Infrastructure.Authentication;
using Echo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

namespace Echo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Включаем поддержку контроллеров
            builder.Services.AddControllers();

            // 2. Включаем Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // 3. Подключаем PostgreSQL
            builder.Services.AddDbContext<EchoDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 4. Связываем интерфейс с контекстом БД
            builder.Services.AddScoped<IEchoDbContext>(provider => provider.GetRequiredService<EchoDbContext>());

            // 5. Подключаем MediatR
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));

            // 6. Подключам JWT
            builder.Services.AddScoped<IJwtProvider, JwtProvider>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["JwtOptions:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["JwtOptions:Audience"],
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["JwtOptions:SecretKey"]!))
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddHttpContextAccessor(); // Включаем доступ к HTTP-контексту
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>(); // Регистрируем наш сервис

            var app = builder.Build();

            // 6. Включаем Swagger только для режима разработки
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}