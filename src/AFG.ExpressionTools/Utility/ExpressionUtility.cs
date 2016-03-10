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
            return ExpressionReplacer.Replace(expression, toReplace.Equals, x => replacement);
        }

        public static Expression Replace(this Expression expression, IReadOnlyDictionary<Expression, Expression> map)
        {
            return ExpressionReplacer.Replace(expression, map.ContainsKey, x => map[x]);
        }

        class ExpressionReplacer : ExpressionVisitor
        {
            private readonly Func<Expression, bool> _match;
            private readonly Func<Expression, Expression> _replace;

            private ExpressionReplacer(Func<Expression, bool> match, Func<Expression, Expression> replace)
            {
                _match = match;
                _replace = replace;
            }

            public override Expression Visit(Expression node)
            {
                return _match(node) ? _replace(node) : base.Visit(node);
            }

            public static Expression Replace(Expression expression, Func<Expression, bool> match,
                Func<Expression, Expression> replace)
            {
                return new ExpressionReplacer(match, replace).Visit(expression);
            }
        }
    }
}
