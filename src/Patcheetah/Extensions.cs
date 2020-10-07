using Patcheetah.Patching;
using System;
using System.Linq.Expressions;

namespace Patcheetah
{
    public static class Extensions
    {
        public static TValue GetValue<TEntity, TValue>(this PatchObject<TEntity> patch, Expression<Func<TEntity, TValue>> propertyGetter)
            where TEntity: class
        {
            var propName = ExpressionHelper.ExtractPropertyName(propertyGetter);

            return (TValue)patch[propName];
        }
    }
}