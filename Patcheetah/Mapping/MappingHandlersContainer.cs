using System;
using System.Collections.Generic;

namespace Patcheetah.Mapping
{
    public class MappingHandlersContainer
    {
        private static MappingHandlersContainer _container;
        private IDictionary<string, MappingHandler> _handlers;

        public MappingHandler GlobalMappingHandler { get; private set; }

        public static MappingHandlersContainer Instance => _container ?? (_container = new MappingHandlersContainer());

        private MappingHandlersContainer()
        {
            _handlers = new Dictionary<string, MappingHandler>();
        }

        public void AddHandler(Type type, MappingHandler handler)
        {
            var typeName = type.Name;

            if (!_handlers.ContainsKey(typeName))
            {
                _handlers.Add(typeName, handler);
            }

            _handlers[typeName] = handler;
        }

        public void AddGlobalHandler(MappingHandler handler)
        {
            GlobalMappingHandler = handler;
        }

        public MappingHandler GetHandler(Type type)
        {
            var typeName = type.Name;

            if (!_handlers.ContainsKey(typeName))
            {
                return null;
            }

            return _handlers[typeName];
        }
    }
}
