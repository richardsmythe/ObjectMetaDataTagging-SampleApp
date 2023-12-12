using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;
using Xunit;

namespace ObjectMetaDataTagging.Test
{
    public class UnitTest1
    {
        [Fact]
        public async Task SetTagAsync_AddsTagToObject()
        {
            // Arrange
            var mockAddedHandler = new Mock<IAsyncEventHandler<AsyncTagAddedEventArgs>>();            
            var taggingEventManager = new TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs>(
                mockAddedHandler.Object,null,null       
            );

            var taggingService = new InMemoryTaggingService<BaseTag>(taggingEventManager);

            var obj = new PersonTranscation { Id = Guid.NewGuid(), Amount = 2000, Sender = "Richard", Receiver = "Jon" };
            var tag = new BaseTag("TestTag", "TestValue");

            // Act
            await taggingService.SetTagAsync(obj, tag);

            // Assert
            Assert.True(taggingService.data.TryGetValue(obj, out var tagDictionary));
            Assert.True(tagDictionary.ContainsKey(tag.Id));

            // Verify the tagDictionary has the correct values
            var tags = await taggingService.GetAllTags(obj);
            Assert.Single(tags);
            Assert.Equal(tag, tags.First());

            // Verify that the event manager was invoked if not null
            mockAddedHandler.Verify(
                handler => handler.HandleAsync(It.IsAny<AsyncTagAddedEventArgs>()),
                Times.AtMostOnce);
        }
    }

    // Sample classes for testing
    public class PersonTranscation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public double Amount { get; set; }

        //public List<BaseTag> AssociatedTags { get; } = new List<BaseTag>();
    }
}
