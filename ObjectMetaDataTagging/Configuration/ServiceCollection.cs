using Microsoft.Extensions.DependencyInjection;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;

namespace ObjectMetaDataTagging.Configuration
{
    /* 
       Allows devs using my library to register all the services at once,
       e.g. they'd use builder.Services.AddObjectMetaDataTagging();
    */
    public static class ServiceCollection
    {
        public static IServiceCollection AddObjectMetaDataTagging(this IServiceCollection services)
        {
            // Register default services
            services.AddScoped<IDefaultTaggingService, DefaultTaggingService>();
            services.AddSingleton<ITagFactory, TagFactory>();

            // Register the EventManager
            services.AddScoped<TaggingEventManager<TagAddedEventArgs, TagRemovedEventArgs, TagUpdatedEventArgs>>();
  

            return services;
        
        }
    }
}
