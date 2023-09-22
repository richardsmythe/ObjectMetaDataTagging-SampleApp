using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Api.Services
{
    public class CustomTaggingService : DefaultTaggingService
    {
        public CustomTaggingService(
            TaggingEventManager<TagAddedEventArgs, TagRemovedEventArgs, TagUpdatedEventArgs> eventManager) 
                : base(eventManager)
        {
        }

        //public override void SetTag(object o, BaseTag tag)
        //{
        //    if (o is ExamplePersonTransaction transaction)
        //    {
        //        transaction.AssociatedTags.Add(tag);
        //    }
        //    //Console.WriteLine($"Tag Id: {tag.Id}   TagName:  {tag.Name}  TagValue:  {tag.Value}");
        //    base.SetTag(o, tag);
        //}

        //public override List<BaseTag> GetAllTags(object o)
        //{
        //    if (o != null && data.TryGetValue(o, out var tags))
        //    {
        //        var allTags = tags.Values.ToList();
        //        //Console.WriteLine("Tags:");
        //        //foreach (var tag in allTags)
        //        //{
        //        //    Console.WriteLine($"- Tag Id: {tag.Id}, Name: {tag.Name}, Value: {tag.Value}");
        //        //}
        //        return allTags;
        //    }
        //    Console.WriteLine("No tags found for the object.");
        //    return new List<BaseTag>();
        //}
    }
}
