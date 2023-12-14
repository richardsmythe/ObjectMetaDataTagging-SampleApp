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
        [Theory]
        [InlineData(1500)]
        [InlineData(2500)]
        public async void SetTagAsync_ShouldAddToDictionary_AndRaiseEventIfConditionIsTrue(int amount)
        {
            // Arrange
            var mockAddedHandler = new Mock<IAsyncEventHandler<AsyncTagAddedEventArgs>>();
            var taggingEventManager = new TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs>(
                mockAddedHandler.Object, null, null
            );

            var taggingService = new InMemoryTaggingService<BaseTag>(taggingEventManager);

            var obj = new PersonTranscation { Id = Guid.NewGuid(), Amount = amount, Sender = "Richard", Receiver = "Jon" };
            var tag = new BaseTag("TestTag", 43, "A numeric tag");

            // Act
            await taggingService.SetTagAsync(obj, tag);

            // Assert
            Assert.True(taggingService.data.TryGetValue(obj, out var tagDictionary));
            Assert.True(tagDictionary.ContainsKey(tag.Id));
            Assert.True(tagDictionary.Count != 0);
            var tags = await taggingService.GetAllTags(obj);
            Assert.Single(tags);
            Assert.Equal(tag, tags.First());

            mockAddedHandler.Verify(
                handler => handler.HandleAsync(It.IsAny<AsyncTagAddedEventArgs>()),
                Times.AtMostOnce);

            // Check if the tag was raised or not depending on given condition
            if (obj.Amount > 2000)
            {
                mockAddedHandler.Verify(
                    handler => handler.HandleAsync(It.Is<AsyncTagAddedEventArgs>(e => e.TaggedObject == obj && e.Tag == tag)),
                    Times.AtMostOnce);
            }
            else
            {
                mockAddedHandler.Verify(
                   handler => handler.HandleAsync(It.Is<AsyncTagAddedEventArgs>(e => e.TaggedObject == obj && e.Tag == tag)),
                   Times.Never);

            }
        }
    }

        public class PersonTranscation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public double Amount { get; set; }

        //public List<BaseTag> AssociatedTags { get; } = new List<BaseTag>();
    }
}
