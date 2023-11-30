using ObjectMetaDataTagging.Api.Events;
using ObjectMetaDataTagging.Api.Services;
using ObjectMetaDataTagging.Configuration;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;

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

            // Web API-specific services
            builder.Services.AddSingleton<IAlertService, AlertService>();
            builder.Services.AddSingleton<IAsyncEventHandler<AsyncTagAddedEventArgs>, TagAddedHandler>();
            builder.Services.AddSingleton<IAsyncEventHandler<AsyncTagRemovedEventArgs>, TagRemovedHandler>();
            builder.Services.AddSingleton<IAsyncEventHandler<AsyncTagUpdatedEventArgs>, TagUpdatedHandler>();

            // Register CustomTaggingService
            builder.Services.AddSingleton<IDefaultTaggingService<BaseTag>, CustomTaggingService<BaseTag>>();


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
