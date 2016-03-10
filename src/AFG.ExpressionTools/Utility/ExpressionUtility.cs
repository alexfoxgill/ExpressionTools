using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

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

        public static PropertyInfo GetPropertyInfo<T1, T2>(this Expression<Func<T1, T2>> propertyGetter)
        {
            var memberExpr = propertyGetter.Body as MemberExpression;
            if (memberExpr == null)
                throw new ArgumentException("Expression should be property getter: " + propertyGetter);
            var propInfo = memberExpr.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException("Expression should be property getter: " + propertyGetter);
            return propInfo;
        }

        public static MemberInitExpression AddBinding(this MemberInitExpression memberInit, MemberBinding binding)
        {
            var bindings = new List<MemberBinding>(memberInit.Bindings) { binding };
            return Expression.MemberInit(memberInit.NewExpression, bindings);
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
