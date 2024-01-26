
using ObjectMetaDataTagging.Api.Interfaces;
using ObjectMetaDataTagging.Api.Services;
using ObjectMetaDataTagging.Configuration;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;

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

            builder.Services.AddControllers();

            // Register OMDT library
            builder.Services.AddObjectMetaDataTagging();

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
