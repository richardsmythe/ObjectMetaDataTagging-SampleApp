using Microsoft.Extensions.DependencyInjection;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.QueryModels;
using System.Diagnostics;

namespace ObjectMetaDataTagging.Configuration
{
    /* 
       Default services for the external application to register with,
       e.g., builder.Services.AddObjectMetaDataTagging();
    */
    public static class ServiceCollection
    {
        public static IServiceCollection AddObjectMetaDataTagging(this IServiceCollection services)
        {
            // Register default services
            services.AddScoped<IDefaultTaggingService, DefaultTaggingService>();

            // Register IDynamicQueryBuilder with its two type parameters
            services.AddScoped(typeof(IDynamicQueryBuilder<,>), typeof(DynamicQueryBuilder<,>));

            services.AddSingleton<ITagFactory, TagFactory>();

            // Register the EventManager
            services.AddScoped<TaggingEventManager<TagAddedEventArgs, TagRemovedEventArgs, TagUpdatedEventArgs>>();

            return services;
        }
    }
}
