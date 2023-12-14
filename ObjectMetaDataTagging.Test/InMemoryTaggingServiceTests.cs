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
            // Set the callback to simulate a situation where the tagAddedEvent is used to add a 'Suspicous' tag
            // if a transaction amount was over 2000. 
            taggingService.OnSetTagAsyncCallback = async (o, t) =>
            {
                if (amount > 2000)
                {
                    taggingService.data.TryGetValue(o, out var tagDictionary);
                    var suspiciousTag = new BaseTag("Suspicious", null, "This object has been tagged as suspicious.");
                    tagDictionary?.TryAdd(suspiciousTag.Id, suspiciousTag);

                    await taggingEventManager.RaiseTagAdded(new AsyncTagAddedEventArgs(o, suspiciousTag));
                }               
            };

            await taggingService.SetTagAsync(obj, tag);

            // Assert
            Assert.True(taggingService.data.TryGetValue(obj, out var tagDictionary));
            Assert.True(tagDictionary.ContainsKey(tag.Id));
            Assert.True(tagDictionary.Count != 0);
            var tags = await taggingService.GetAllTags(obj);
            Assert.True(tagDictionary.Count == tags.Count());
            Assert.Equal(tag, tags.First());

            if (obj.Amount > 2000)
            {
                mockAddedHandler.Verify(
                    handler => handler.HandleAsync(It.Is<AsyncTagAddedEventArgs>(e => e.TaggedObject == obj && e.Tag.Name == "Suspicious")),
                    Times.AtMostOnce);
                var suspiciousTagId = tagDictionary.Values.FirstOrDefault(t => t.Name == "Suspicious")!.Id;
                Assert.Equal("Suspicious", tagDictionary[suspiciousTagId].Name);
                Assert.Equal(suspiciousTagId, tagDictionary[suspiciousTagId].Id);
                Assert.Equal("This object has been tagged as suspicious.", tagDictionary[suspiciousTagId].Description);
            }
            else
            {
                mockAddedHandler.Verify(
                    handler => handler.HandleAsync(It.IsAny<AsyncTagAddedEventArgs>()),
                    Times.Never);
                Assert.DoesNotContain("Suspicious", tagDictionary.Values.Select(t => t.Name));
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
