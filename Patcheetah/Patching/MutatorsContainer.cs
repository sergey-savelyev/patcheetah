using System;
using System.Collections.Generic;
using System.Linq;

namespace Patcheetah.Patching
{
    internal class MutatorsContainer
    {
        private static MutatorsContainer _container;

        public static MutatorsContainer Instance => _container ??
            (_container = new MutatorsContainer());

        public IPatchingPropertyValueMutator[] Mutators => _mutators.ToArray();

        private List<IPatchingPropertyValueMutator> _mutators;

        private MutatorsContainer()
        {
            _mutators = new List<IPatchingPropertyValueMutator>();
        }

        public void AddMutator(IPatchingPropertyValueMutator mutator)
        {
            if (_mutators.Any(x => x.Id == mutator.Id))
            {
                throw new Exception($"Patching mutator with id {mutator.Id} already added");
            }

            _mutators.Add(mutator);
        }
    }
}
