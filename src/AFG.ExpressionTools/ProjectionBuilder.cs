using AFG.ExpressionTools.Utility;
using System;
using System.Linq.Expressions;

namespace AFG.ExpressionTools
{
    public class Projection
    {
        /// <summary>
        /// Creates a <see cref="ProjectionBuilder{TSource,TDest}"/> using a <see cref="NewExpression"/> or <see cref="MemberInitExpression"/> as a base
        /// </summary>
        /// <typeparam name="TSource">The source type of the projection</typeparam>
        /// <typeparam name="TDest">The destination type of the projection</typeparam>
        /// <param name="expr">The initial projection expression</param>
        /// <returns>A Projection Builder</returns>
        public static ProjectionBuilder<TSource, TDest> Create<TSource, TDest>(Expression<Func<TSource, TDest>> expr)
            where TDest : new()
        {
            return new ProjectionBuilder<TSource, TDest>(expr);
        }

        /// <summary>
        /// Creates a <see cref="ProjectionBuilder{TSource,TDest}"/> with a <see cref="NewExpression"/> as a base
        /// </summary>
        /// <typeparam name="TSource">The source type of the projection</typeparam>
        /// <typeparam name="TDest">The destination type of the projection</typeparam>
        /// <returns>A Projection Builder</returns>
        public static ProjectionBuilder<TSource, TDest> Create<TSource, TDest>()
            where TDest : new()
        {
            return new ProjectionBuilder<TSource, TDest>();
        }

        /// <summary>
        /// Entry point into specifying a <see cref="ProjectionBuilder{TSource,TDest}"/> using implicit destination type
        /// </summary>
        /// <typeparam name="TSource">The source type of the projection</typeparam>
        /// <returns>An intermediate object which can produce a ProjectionBuilder</returns>
        public static IntermediateProjectionBuilder<TSource> From<TSource>()
        {
            return new IntermediateProjectionBuilder<TSource>();
        }

        /// <summary>
        /// An intermediate class which can produce a <see cref="ProjectionBuilder{TSource,TDest}"/>
        /// </summary>
        /// <typeparam name="TSource">The source type of the projection</typeparam>
        public class IntermediateProjectionBuilder<TSource>
        {

            /// <summary>
            /// Creates a <see cref="ProjectionBuilder{TSource,TDest}"/> using an implicit destination type
            /// </summary>
            /// <typeparam name="TDest">The destination type of the projection</typeparam>
            /// <returns>A Projection Builder</returns>
            public ProjectionBuilder<TSource, TDest> To<TDest>(Expression<Func<TSource, TDest>> expr) where TDest : new()
            {
                return Create(expr);
            }
        }

        /// <summary>
        /// Builds a projection from one type to another
        /// </summary>
        /// <typeparam name="TSource">The source type of the projection</typeparam>
        /// <typeparam name="TDest">The destination type of the projection</typeparam>
        public class ProjectionBuilder<TSource, TDest> where TDest : new()
        {
            /// <summary>
            /// Creates a <see cref="ProjectionBuilder{TSource,TDest}"/> using using a <see cref="NewExpression"/> or <see cref="MemberInitExpression"/> as a base
            /// </summary>
            /// <param name="expr">The initial projection expression</param>
            public ProjectionBuilder(Expression<Func<TSource, TDest>> expr)
            {
                if (!(expr.Body is MemberInitExpression || expr.Body is NewExpression))
                    throw new ArgumentException("Expression must be MemberInitExpression or NewExpression");
                GetExpression = expr;
            }

            /// <summary>
            /// Creates a <see cref="ProjectionBuilder{TSource,TDest}"/> with a <see cref="NewExpression"/> as a base
            /// </summary>
            public ProjectionBuilder()
                : this(GenerateConstructorExpression())
            {
            }

            /// <summary>
            /// The resultant projection expression
            /// </summary>
            public Expression<Func<TSource, TDest>> GetExpression { get; }

            private static Expression<Func<TSource, TDest>> GenerateConstructorExpression()
            {
                var parameter = Expression.Parameter(typeof(TSource));
                var ctor = typeof(TDest).GetConstructor(new Type[0]);
                var newExpr = Expression.New(ctor);
                return Expression.Lambda<Func<TSource, TDest>>(newExpr, parameter);
            }

            public static implicit operator Expression<Func<TSource, TDest>>(
                ProjectionBuilder<TSource, TDest> projectionBuilder)
            {
                return projectionBuilder.GetExpression;
            }

            /// <summary>
            /// Adds a projection for a specific member
            /// </summary>
            /// <typeparam name="TProp">The type of the member</typeparam>
            /// <param name="getter">A <see cref="MemberExpression"/> declaring the property to set</param>
            /// <param name="propValueExpr">The expression to set as the value of the property</param>
            /// <returns>A new ProjectionBuilder with the additional projection</returns>
            public ProjectionBuilder<TSource, TDest> With<TProp>(Expression<Func<TDest, TProp>> getter,
                Expression<Func<TSource, TProp>> propValueExpr)
            {
                var memberBinding = CreateMemberBinding(getter, propValueExpr);
                var expr = CreateExpression(memberBinding);
                var lambda = Expression.Lambda<Func<TSource, TDest>>(expr, GetExpression.Parameters);
                return new ProjectionBuilder<TSource, TDest>(lambda);
            }

            private MemberBinding CreateMemberBinding<TProp>(Expression<Func<TDest, TProp>> getter,
                Expression<Func<TSource, TProp>> propValueExpr)
            {
                var expr = propValueExpr.Body.Replace(propValueExpr.Parameters[0], GetExpression.Parameters[0]);
                var propertyInfo = getter.GetPropertyInfo();
                return Expression.Bind(propertyInfo, expr);
            }

            private Expression CreateExpression(MemberBinding newMemberBinding)
            {
                var newExpr = GetExpression.Body as NewExpression;
                if (newExpr != null)
                {
                    return Expression.MemberInit(newExpr, newMemberBinding);
                }

                var memberInitExpr = GetExpression.Body as MemberInitExpression;
                if (memberInitExpr != null)
                {
                    return memberInitExpr.AddBinding(newMemberBinding);
                }

                throw new InvalidOperationException("Constructor invariant breached");
            }
        }
    }
}
