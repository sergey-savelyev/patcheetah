using Patcheetah.Configuration;
using Patcheetah.Patching;

namespace Patcheetah
{
    public class ExtendedSettingsService
    {
        public void AddConfigurationBehaviour(IConfigurationBehaviour behaviour)
        {
            ConfigurationContainer.Instance.RegisterBehaviour(behaviour);
        }

        public void AddPatchingPropertyValueMutator(IPatchingPropertyValueMutator mutator)
        {
            MutatorsContainer.Instance.AddMutator(mutator);
        }
    }
}
