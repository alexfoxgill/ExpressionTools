using System;
using System.Linq.Expressions;

namespace AFG.ExpressionTools.Utility
{
    public static class UtilityExtensions
    {
        public static bool IsConstant<T, TResult>(Expression<Func<T, TResult>> expr, TResult value)
        {
            var constant = expr.Body as ConstantExpression;
            if (constant == null)
                return false;
            return constant.Value.Equals(value);
        }
    }
}
