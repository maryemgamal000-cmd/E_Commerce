using E_Commerce.API.Extensions;
using E_Commerce.Domain.Contracts;
using E_Commerce.Infrastructure;
using E_Commerce.Application;
using E_Commerce.Application.Profiles;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.FileProviders;
using E_Commerce.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using E_Commerce.Infrastructure.Identity.Services;
using E_Commerce.Application.Contracts;
using E_Commerce.Application.Common;

namespace E_Commerce.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddInfrastructureServices(builder.Configuration);
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddApplicationServices();
            builder.Services.Configure<UrlSettings>(builder.Configuration.GetSection("UrlSettings"));
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JWT"));
            builder.Services.Configure<PaymentGatewaySettings>(builder.Configuration.GetSection("Stripe"));



            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
           


            var app = builder.Build();

            await app.SeedAndMigrateDataAsync();      

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseStaticFiles(new StaticFileOptions
             {
                   FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath , "Files")),
                   RequestPath= "/Files" 

             } );   

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
