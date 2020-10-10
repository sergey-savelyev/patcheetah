namespace Patcheetah.Configuration
{
    public class EntityConfigAccessor
    {
        public EntityConfig EntityConfig { get; }

        internal EntityConfigAccessor(EntityConfig entityConfig)
        {
            EntityConfig = entityConfig;
        }

        public PropertyConfiguration GetPropertyConfiguration(string propertyName)
        {
            return EntityConfig.GetOrCreatePropertyConfiguration(propertyName);
        }
    }
}
