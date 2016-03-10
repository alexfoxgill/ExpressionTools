using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AFG.ExpressionTools.Utility
{
    public static class ExpressionUtility
    {
        public static bool IsConstant<T, TResult>(Expression<Func<T, TResult>> expr, TResult value)
        {
            var constant = expr.Body as ConstantExpression;
            if (constant == null)
                return false;
            return constant.Value.Equals(value);
        }

        public static Expression Replace(this Expression expression, Expression toReplace, Expression replacement)
        {
            var map = new Dictionary<Expression, Expression>() { { toReplace, replacement } };
            return ExpressionReplacer.Replace(expression, map);
        }

        public static Expression Replace(this Expression expression, IReadOnlyDictionary<Expression, Expression> replacementMap)
        {
            return ExpressionReplacer.Replace(expression, replacementMap);
        }

        class ExpressionReplacer : ExpressionVisitor
        {
            private readonly IReadOnlyDictionary<Expression, Expression> _replacementMap;

            private ExpressionReplacer(IReadOnlyDictionary<Expression, Expression> replacementMap)
            {
                _replacementMap = replacementMap;
            }

            public override Expression Visit(Expression node)
            {
                Expression replace;
                if (_replacementMap.TryGetValue(node, out replace))
                    return replace;
                return base.Visit(node);
            }

            public static Expression Replace(Expression expression, IReadOnlyDictionary<Expression, Expression> replacementMap)
            {
                return new ExpressionReplacer(replacementMap).Visit(expression);
            }
        }
    }
}
