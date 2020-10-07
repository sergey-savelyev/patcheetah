using System;
using System.Linq.Expressions;

namespace Patcheetah
{
    internal static class ExpressionHelper
    {
        public static string ExtractPropertyName<TEntity, TReturn>(Expression<Func<TEntity, TReturn>> property)
        {
            if (!(property.Body is MemberExpression memberExpression))
            {
                throw new ArgumentException("Only properties can be patched", property.Name);
            }

            return memberExpression.Member.Name;
        }
    }
}
