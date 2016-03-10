using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AFG.ExpressionTools
{
    public static class EntityFrameworkFixExtensions
    {
        public static Expression<Func<TIn, TResult>> FixSumExpression<TIn, TResult>(this Expression<Func<TIn, TResult>> expr)
        {
            return EmptyCollectionSumFixer.Replace(expr);
        }

        class EmptyCollectionSumFixer : ExpressionVisitor
        {
            private EmptyCollectionSumFixer() { }

            private static readonly IEnumerable<MethodInfo> SumMethods =
                typeof(Enumerable)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(x => x.Name == "Sum");

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (SumMethods.All(x => x.MetadataToken != node.Method.MetadataToken)
                    || node.Method.ReturnType.IsGenericType)
                    return base.VisitMethodCall(node);

                var source = node.Arguments[0];
                if (node.Arguments.Count == 2)
                {
                    var selector = node.Arguments[1] as LambdaExpression;
                    return Create(source, selector.ReturnType, selector.Body, selector.Parameters.ToArray());
                }
                else
                {
                    var returnType = source.Type.GetGenericArguments()[0];
                    var parameter = Expression.Parameter(returnType);
                    return Create(source, returnType, parameter, parameter);
                }
            }

            private static Expression Create(Expression source, Type returnType, Expression lambdaBody, params ParameterExpression[] lambdaParameters)
            {
                var sourceType = source.Type.GetGenericArguments()[0];
                var nullableType = typeof(Nullable<>).MakeGenericType(returnType);
                var nullableSumMethod = SumMethods
                    .First(x => x.GetParameters().Count() == 2 && x.ReturnType == nullableType)
                    .MakeGenericMethod(sourceType);

                var nullableBody = Expression.Convert(lambdaBody, nullableType);
                var nullableLambdaExpression = Expression.Lambda(nullableBody, lambdaParameters);

                return Expression.Coalesce(
                    Expression.Call(nullableSumMethod, source, nullableLambdaExpression),
                    Expression.Constant(Activator.CreateInstance(returnType)));
            }

            public static Expression<Func<TIn, TResult>> Replace<TIn, TResult>(Expression<Func<TIn, TResult>> expr)
            {
                var result = new EmptyCollectionSumFixer().Visit(expr.Body);
                return Expression.Lambda<Func<TIn, TResult>>(result, expr.Parameters);
            }
        }
    }
}
