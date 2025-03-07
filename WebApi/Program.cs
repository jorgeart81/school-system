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

            builder.Services.AddInfrastructureServices(builder.Configuration);

            builder.Services.AddJwtAuthetication(builder.Services.GetJwtSettings(builder.Configuration));

            var app = builder.Build();

            // Database Seeder
            await app.Services.AddDatabaseInitializerAsync();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseInfrastructure();

            app.MapControllers();

            app.Run();
        }
    }
}
