using System;
using System.Collections.Generic;
using MatchPuzzle.Core.Interfaces;

namespace MatchPuzzle.ApplicationLayerLayer.EventBus
{
    /// <summary>
    /// Global event bus implementation
    /// </summary>
    public class GlobalEventBus : IGlobalEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscriptions = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            var eventType = typeof(TEvent);

            if (!_subscriptions.ContainsKey(eventType))
            {
                _subscriptions[eventType] = new List<Delegate>();
            }

            _subscriptions[eventType].Add(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            var eventType = typeof(TEvent);

            if (_subscriptions.ContainsKey(eventType))
            {
                _subscriptions[eventType].Remove(handler);
            }
        }

        public void Publish<TEvent>(TEvent eventData) where TEvent : class
        {
            var eventType = typeof(TEvent);

            if (!_subscriptions.ContainsKey(eventType))
                return;

            var handlers = _subscriptions[eventType];

            foreach (var handler in handlers)
            {
                (handler as Action<TEvent>)?.Invoke(eventData);
            }
        }

        public void Clear()
        {
            _subscriptions.Clear();
        }
    }
}
