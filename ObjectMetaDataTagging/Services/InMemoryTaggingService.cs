using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Exceptions;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;
using System.Collections.Concurrent;
using System.Threading;


namespace ObjectMetaDataTagging.Services
{

    public class InMemoryTaggingService<T> : IDefaultTaggingService<T>
        where T : BaseTag
    {
        /// <summary>
        /// Constructs a DefaultTaggingService with the specified TaggingEventManager for event handling.
        /// </summary>
        /// <param name="eventManager">The TaggingEventManager used for handling tagging-related events.</param>
        public InMemoryTaggingService(
            TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs> eventManager)
        {
            _eventManager = eventManager;
        }

        public InMemoryTaggingService()
        {
            _eventManager = null;
        }

        // using object instead of weakreference, make sure to GC
        public readonly ConcurrentDictionary<object, Dictionary<Guid, BaseTag>> data = new ConcurrentDictionary<object, Dictionary<Guid, BaseTag>>();
        public readonly TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs> _eventManager;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        /* By exposing these events, it allow consumers to attach event handlers 
         * to perform additional actions when tags are added, removed, or updated. 
         * This can be useful if someone wants to extend the behavior of the library. */

        //public event EventHandler<AsyncTagAddedEventArgs> TagAdded
        //{
        //    add => _eventManager.TagAdded += value;
        //    remove => _eventManager.TagAdded -= value;
        //}

        //public event EventHandler<AsyncTagRemovedEventArgs> TagRemoved
        //{
        //    add => _eventManager.TagRemoved += value;
        //    remove => _eventManager.TagRemoved -= value;
        //}

        //public event EventHandler<AsyncTagUpdatedEventArgs> TagUpdated
        //{
        //    add => _eventManager.TagUpdated += value;
        //    remove => _eventManager.TagUpdated -= value;
        //}

        #region Default Tag Operations
        public virtual Task<IEnumerable<T>> GetAllTags(object o)
        {
            if (o == null)
            {
                throw new ObjectNotFoundException("No object supplied.");
            }

            if (data.TryGetValue(o, out var tags))
            {
                var allTags = tags.Values.ToList();
                Console.WriteLine("Tags:");
                foreach (var tag in allTags)
                {
                    if (tag is T typedTag)
                    {
                        Console.WriteLine($"- Tag Id: {typedTag.Id}, Name: {typedTag.Name}, Value: {typedTag.Value}");
                    }
                }
                return Task.FromResult((IEnumerable<T>)allTags);
            }

            throw new ObjectNotFoundException("Object not found in the data collection.", nameof(o));
        }

        public virtual Task<T>? GetTag(object o, Guid tagId)
        {
            if (o == null)
            {
                throw new ObjectNotFoundException("No object supplied.");
            }

            if (data.TryGetValue(o, out var tagDictionary))
            {
                if (tagDictionary.TryGetValue(tagId, out var tag))
                {
                    return tag as Task<T>;
                }
            }

            throw new ObjectNotFoundException("Object or tag not found in the data collection.", nameof(o));
        }

        public virtual async Task<bool> RemoveAllTagsAsync(object o)
        {
            await semaphore.WaitAsync();
            try
            {
                if (o == null)
                {
                    throw new ObjectNotFoundException("No object supplied.");
                }

                if (data.Remove(o, out _))
                {
                    return true;
                }

                return false;
      
            }
            finally
            {
                semaphore.Release();
            }    
        }

        public virtual async Task<bool> RemoveTagAsync(object? o, Guid tagId)
        {
            await semaphore.WaitAsync();

            try
            {
                if (o == null)
                {
                    throw new ObjectNotFoundException("No object supplied.");
                }

                if (data.TryGetValue(o, out var tags))
                {
                    if (tags.Remove(tagId))
                    {
                        // if there are no tags left for this object, remove entry
                        if (tags.Count == 0)
                        {
                            data.TryRemove(o, out _);
                        }

                        return true;
                    }

                    await _eventManager.RaiseTagRemoved(new AsyncTagRemovedEventArgs(o, tagId));
                }
                else
                {
                    throw new ObjectNotFoundException("Object not found in the data collection.", nameof(o));
                }
            }
            finally
            {
                semaphore.Release();
            }
            return false;
        }

        public virtual async Task SetTagAsync(object o, T tag)
        {
            if (tag == null || o == null) throw new ObjectNotFoundException("No object or tag supplied.");

            var objectName = o.GetType().Name;
            var objectId = GetObjectId(o);

            var tagDictionary = data.GetOrAdd(o, new Dictionary<Guid, BaseTag>());

            // ensure that multiple threads don't interfere with
            // each other when modifying the dictionary concurrently
            await semaphore.WaitAsync();
            try
            {
                tagDictionary[tag.Id] = tag;
                tag.AssociatedParentObjectId = objectId;
                tag.AssociatedParentObjectName = objectName;

            }
            finally
            {
                semaphore.Release();
            }
        }

        public virtual async Task<bool> UpdateTagAsync(object o, Guid tagId, T modifiedTag)
        {
            if (modifiedTag == null || o == null)
            {
                throw new ObjectNotFoundException("No object or tag supplied.");
            }

            modifiedTag.DateLastUpdated = DateTime.UtcNow;

            // Ensure that multiple threads don't interfere with
            // each other when modifying the dictionary concurrently
            await semaphore.WaitAsync();

            try
            {
                if (data.TryGetValue(o, out var tags))
                {
                    if (tags.ContainsKey(tagId))
                    {
                        tags[tagId] = modifiedTag;
                        return true;
                    }
                }

                await _eventManager.RaiseTagUpdated(new AsyncTagUpdatedEventArgs(o, null, modifiedTag));
                return false;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public bool HasTag(object o, Guid tagId)
        {
            if (o != null && data.TryGetValue(o, out var tags))
            {
                lock (tags)
                {
                    if (tags.ContainsKey(tagId))
                    {
                        return true;
                    }
                    else
                    {
                        throw new ObjectNotFoundException($"Tag with Id {tagId} not found for the specified object.", nameof(tagId));
                    }
                }
            }
            else
            {
                throw new ObjectNotFoundException("Object not found in the data dictionary.", nameof(o));
            }
        }

        public virtual Task<T?> GetObjectByTag(Guid tagId)
        {
            foreach (var kvp in data)
            {
                var tags = kvp.Value;

                lock (tags)
                {
                    if (tags.TryGetValue(tagId, out var tag))
                    {
                        if (kvp.Key is Task<T> associatedObject)
                        {
                            return associatedObject;
                        }
                        else
                        {
                            throw new ObjectNotFoundException("Associated object is not of expected type Task<T>.", nameof(kvp.Key));
                        }
                    }
                }
            }

            throw new ObjectNotFoundException($"No object found with tagId: {tagId}", nameof(tagId));
        }

        private Guid GetObjectId(object o)
        {
            if (o == null)
            {
                throw new ObjectNotFoundException("No object supplied.");
            }

            var idProperty = o.GetType().GetProperty("Id");
            if (idProperty != null)
            {
                var idValue = idProperty.GetValue(o);
                if (idValue is Guid guidValue)
                {
                    return guidValue;
                }
            }

            throw new ObjectNotFoundException("Id property not found or not of type Guid.", nameof(o));
        }

        #endregion

    }
}
