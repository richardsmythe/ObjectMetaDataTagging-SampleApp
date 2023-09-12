using Microsoft.Extensions.DependencyInjection;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.QueryModels;
using System.Diagnostics;

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
            services.AddScoped<IDynamicQueryBuilder<DefaultFilterCriteria>, DynamicQueryBuilder<DefaultFilterCriteria>>();


            services.AddSingleton<ITagFactory, TagFactory>();

            // Register the EventManager
            services.AddScoped<TaggingEventManager<TagAddedEventArgs, TagRemovedEventArgs, TagUpdatedEventArgs>>();
            
            return services;        
        }
    }
}
