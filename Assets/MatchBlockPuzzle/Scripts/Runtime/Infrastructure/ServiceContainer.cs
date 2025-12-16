using System;
using System.Collections.Generic;

namespace MatchPuzzle.Infrastructure
{
    public class ServiceContainer
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void Register<TService>(TService service) where TService: class
        {
            var type = typeof(TService);
            if (_services.ContainsKey(type))
            {
                throw new InvalidOperationException($"Type {type.Name} is already registered.");
            }
            _services[type] = service;
        }

        public TService Get<TService>() where TService: class
        {
            var type = typeof(TService);
            if (_services.TryGetValue(type, out var service))
            {
                return service as TService;
            }
            throw new InvalidOperationException($"Type {type.Name} is not registered.");
        }

        public bool TryGet<TService>(out TService service) where TService: class
        {
            var type = typeof(TService);
            if (_services.TryGetValue(type, out var obj))
            {
                service = obj as TService;
                return service != null;
            }
            service = null;
            return false;
        }

        public void Clear()
        {
            _services.Clear();
        }
    }
}