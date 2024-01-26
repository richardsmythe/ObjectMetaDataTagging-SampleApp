//using System;
//using System.Threading.Tasks;
//using ObjectMetaDataTagging.Events;
//using ObjectMetaDataTagging.Interfaces;
//using ObjectMetaDataTagging.Models.TagModels;

//namespace ObjectMetaDataTagging.Api.Services
//{
//    public class CustomTaggingService<T> : ObjectMetaDataTaggingFacade<T> where T : BaseTag
//    {
//        public CustomTaggingService(IDefaultTaggingService<T> taggingService)
//            : base(taggingService)
//        {
           
//        }

//        public override async Task SetTagAsync(object o, T tag)
//        {
//            await base.SetTagAsync(o, tag);

//            // Raise the TagAdded event
//            var args = new AsyncTagAddedEventArgs(o, tag);


//            // Check if the event has modified the tag
//            var tagFromEvent = args.Tag;
//            if (tagFromEvent != null)
//            {
//                tagFromEvent.DateLastUpdated = DateTime.UtcNow;
//                tagFromEvent.Parents.Add(tag.Id);

//                // Update the tag using the modified tag from the event
//                await base.SetTagAsync(o, tag);
//            }
//        }

      
//    }
//}
