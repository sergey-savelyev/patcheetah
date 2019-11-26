namespace Modelee.Configuration
{
    public static class ModeleeConfig
    {
        public static EntityConfigTyped<TEntity> CreateFor<TEntity>(bool caseSensitive = false)
            => new EntityConfigTyped<TEntity>(caseSensitive);
    }
}
