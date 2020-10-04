using Patcheetah.Configuration;
using System.Collections.Generic;

namespace Patcheetah.Patching
{
    public interface IEntityPatcher
    {
        public void Patch<TEntity>(TEntity entityToPatch, IDictionary<string, object> patchData, EntityConfig config)
             where TEntity : class;

        public TEntity BuildNew<TEntity>(IDictionary<string, object> patchData, EntityConfig config)
            where TEntity: class;
    }
}
