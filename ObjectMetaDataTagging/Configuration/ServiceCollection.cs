using Microsoft.Extensions.DependencyInjection;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;

namespace ObjectMetaDataTagging.Configuration
{
    /* 
       Default services for external application to register with,
       eg.g. builder.Services.AddObjectMetaDataTagging();
    */
    public static class ServiceCollection
    {
        public static IServiceCollection AddObjectMetaDataTagging(this IServiceCollection services)
        {
            // Register default services
            services.AddScoped<IDefaultTaggingService, DefaultTaggingService>();
            services.AddScoped<IDynamicQueryBuilder, DynamicQueryBuilder>();
            services.AddSingleton<ITagFactory, TagFactory>();

            // Register the EventManager
            services.AddScoped<TaggingEventManager<TagAddedEventArgs, TagRemovedEventArgs, TagUpdatedEventArgs>>();
            
            return services;        
        }
    }
}
