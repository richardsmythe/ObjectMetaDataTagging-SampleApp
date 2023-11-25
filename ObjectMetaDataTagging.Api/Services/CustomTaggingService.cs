﻿using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Api.Services
{
    public class CustomTaggingService<T> : DefaultTaggingService<T>
        where T : BaseTag
    {
        public CustomTaggingService(
            TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs> eventManager) 
                : base(eventManager)
        {
        }

        public override async Task SetTag(object o, T tag)
        {
            if (o is ExamplePersonTransaction transaction)
            {
                transaction.AssociatedTags.Add(tag);
            }
            //Console.WriteLine($"Tag Id: {tag.Id}   TagName:  {tag.Name}  TagValue:  {tag.Value}");
           await base.SetTag(o, tag);
        }      
    }
}
