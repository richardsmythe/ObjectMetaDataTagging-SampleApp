using Microsoft.Extensions.DependencyInjection;
using ObjectMetaDataTagging.Interfaces;

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
            services.AddSingleton<IDefaultTaggingService, DefaultTaggingService>();
            services.AddSingleton<ITagFactory, TagFactory>();

            return services;
        }
    }
}
