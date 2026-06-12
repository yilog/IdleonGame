using System;
using System.Collections.Generic;

namespace IdleonGame.UI
{
    public sealed class UIEventBus
    {
        private readonly Dictionary<Type, Delegate> handlers = new();

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                return;
            }

            var eventType = typeof(TEvent);
            handlers[eventType] = handlers.TryGetValue(eventType, out var current)
                ? Delegate.Combine(current, handler)
                : handler;
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                return;
            }

            var eventType = typeof(TEvent);
            if (!handlers.TryGetValue(eventType, out var current))
            {
                return;
            }

            var next = Delegate.Remove(current, handler);
            if (next == null)
            {
                handlers.Remove(eventType);
                return;
            }

            handlers[eventType] = next;
        }

        public void Publish<TEvent>(TEvent payload)
        {
            if (handlers.TryGetValue(typeof(TEvent), out var handler))
            {
                ((Action<TEvent>)handler)?.Invoke(payload);
            }
        }
    }
}
