﻿using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Events
{
    /// <summary>
    /// Manages events related to tagging actions, allowing event handlers to be attached to tagging events.
    /// </summary>
    /// <typeparam name="TAdded">The type of event arguments for tag added events.</typeparam>
    /// <typeparam name="TRemoved">The type of event arguments for tag removed events.</typeparam>
    /// <typeparam name="TUpdated">The type of event arguments for tag updated events.</typeparam>

    public class TaggingEventManager<TAdded, TRemoved, TUpdated>
        where TAdded : AsyncTagAddedEventArgs
        where TRemoved : AsyncTagRemovedEventArgs
        where TUpdated : AsyncTagUpdatedEventArgs
    {

        public event EventHandler<AsyncTagAddedEventArgs>? TagAdded;
        public event EventHandler<AsyncTagRemovedEventArgs>? TagRemoved;
        public event EventHandler<AsyncTagUpdatedEventArgs>? TagUpdated;

        private readonly IAsyncEventHandler<TAdded> _addedHandler;
        private readonly IAsyncEventHandler<TRemoved> _removedHandler;
        private readonly IAsyncEventHandler<TUpdated> _updatedHandler;


        public TaggingEventManager(IAsyncEventHandler<TAdded> addedHandler,
                               IAsyncEventHandler<TRemoved> removedHandler,
                               IAsyncEventHandler<TUpdated> updatedHandler)
        {
            _addedHandler = addedHandler;
            _removedHandler = removedHandler;
            _updatedHandler = updatedHandler;
        }

        public async Task<BaseTag> RaiseTagAdded(TAdded e)
        {
            try
            {
                var result = await _addedHandler.HandleAsync(e);

                if (result != null)
                {
                    if (TagAdded != null)
                    {
                        foreach (var handler in TagAdded.GetInvocationList())
                        {
                            if (handler is IAsyncEventHandler<AsyncTagAddedEventArgs> asyncHandler)
                            {
                                await asyncHandler.HandleAsync(e);
                            }
                        }
                    }

                    return result;
                }
                else
                {
                    // Handle the case where _addedHandler.HandleAsync returns null
                    // You can return a default value or handle it as appropriate for your application
                    Console.WriteLine("HandleAsync returned null in RaiseTagAdded");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as appropriate for your application
                Console.WriteLine($"Exception in RaiseTagAdded: {ex}");
                // Optionally rethrow the exception if further handling is needed
                throw;
            }
        }

        public async Task RaiseTagRemoved(TRemoved e)
        {
            await _removedHandler.HandleAsync(e);

            if (TagRemoved != null)
            {
                foreach (var handler in TagRemoved.GetInvocationList())
                {
                    if (handler is IAsyncEventHandler<AsyncTagRemovedEventArgs> asyncHandler)
                    {
                        TagRemoved?.Invoke(this, e);
                    }
                }
            }
        }
        public async Task RaiseTagUpdated(TUpdated e)
        {
            await _updatedHandler.HandleAsync(e);

            if (TagUpdated != null)
            {
                foreach (var handler in TagUpdated.GetInvocationList())
                {
                    if (handler is IAsyncEventHandler<AsyncTagUpdatedEventArgs> asyncHandler)
                    {
                        TagUpdated?.Invoke(this, e);
                    }
                }
            }
        }

    }
}
