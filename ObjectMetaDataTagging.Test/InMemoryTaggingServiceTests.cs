using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;
using Xunit;
using static ObjectMetaDataTagging.Test.InMemoryTaggingServiceTests;

namespace ObjectMetaDataTagging.Test
{
    public class InMemoryTaggingServiceTests
    {
        [Fact]
        public async void SetTagAsync_ShouldAddToDictionary_AndTriggerEvent()
        {
            // Arrange
            var mockAddedHandler = new Mock<IAsyncEventHandler<AsyncTagAddedEventArgs>>();
            var taggingEventManager = new TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs>(
                mockAddedHandler.Object, null, null
            );

            var taggingService = new InMemoryTaggingService<BaseTag>(taggingEventManager);

            var obj = new PersonTranscation { Id = Guid.NewGuid(), Amount = 2500, Sender = "Richard", Receiver = "Jon" };
            var tag = new BaseTag("TestTag", 43, "A numeric tag");

            // Act
            taggingService.SetTagAsync(obj, tag);

            // Assert
            Assert.True(taggingService.data.TryGetValue(obj, out var tagDictionary));
            Assert.True(tagDictionary.ContainsKey(tag.Id));
            Assert.True(tagDictionary.Count != 0);

            // Verify the tagDictionary has the correct values
            var tags = await taggingService.GetAllTags(obj);
            Assert.Single(tags);
            Assert.Equal(tag, tags.First());

            // Verify that the event manager was invoked if not null and amount is over 2000
            mockAddedHandler.Verify(
                handler => handler.HandleAsync(It.IsAny<AsyncTagAddedEventArgs>()),
                Times.AtMostOnce);

            if (obj.Amount > 2000)
            {
                // If the condition is met, ensure that the event was raised
                mockAddedHandler.Verify(
                    handler => handler.HandleAsync(It.Is<AsyncTagAddedEventArgs>(e => e.TaggedObject == obj && e.Tag == tag)),
                    Times.AtMostOnce);
            }
            else
            {
                // If the condition is not met, ensure that the event was not raised
                mockAddedHandler.Verify(
                    handler => handler.HandleAsync(It.IsAny<AsyncTagAddedEventArgs>()),
                    Times.Never);
            }
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
