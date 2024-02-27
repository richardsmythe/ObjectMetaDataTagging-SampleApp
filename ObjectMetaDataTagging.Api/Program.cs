using ObjectMetaDataTagging.Api.Interfaces;
using ObjectMetaDataTagging.Api.Services;
using ObjectMetaDataTagging.Configuration;
using System.Text.Json.Serialization;

namespace ObjectMetaDataTagging.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(op =>
            {
                op.AddPolicy("AllowAll", b =>
                b.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()
                .AllowAnyMethod());
            });

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve; // or ReferenceHandler.Ignore
            });

            // Register OMDT library
            builder.Services.AddObjectMetaDataTagging();

            // Web API-specific services     
            builder.Services.AddScoped<IGenerateTestData, GenerateTestData>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors("AllowAll");

            app.MapControllers();

            app.Run();
        }
    }
}
