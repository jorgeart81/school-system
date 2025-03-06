using Infrastructure;

namespace WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddSwaggerGen();

            builder.Services.AddInfrastructureServices(builder.Configuration);

            builder.Services.AddJwtAuthetication(builder.Services.GetJwtSettings(builder.Configuration));

            var app = builder.Build();

            // Database Seeder
            await app.Services.AddDatabaseInitializerAsync();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseInfrastructure();

            app.MapControllers();

            app.Run();
        }
    }
}
