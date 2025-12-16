using System;

namespace MatchPuzzle.Core.Interfaces
{
    /// <summary>
    /// Global event bus for decoupled communication
    /// (Useful for analytics, achievements, etc.)
    /// </summary>
    public interface IGlobalEventBus
    {
        /// <summary>
        /// Subscribes to an event
        /// </summary>
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

        /// <summary>
        /// Unsubscribes from an event
        /// </summary>
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

        /// <summary>
        /// Publishes an event
        /// </summary>
        void Publish<TEvent>(TEvent eventData) where TEvent : class;

        /// <summary>
        /// Clears all subscriptions
        /// </summary>
        void Clear();
    }
}
