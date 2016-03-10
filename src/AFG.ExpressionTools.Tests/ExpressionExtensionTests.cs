using FluentAssertions;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Linq.Expressions;
using Xunit;

namespace AFG.ExpressionTools.Tests
{
    public class ExpressionExtensionTests
    {
        [Theory, AutoData]
        public void CombineAppliesCombinationFunction(int i, int j)
        {
            // arrange
            Expression<Func<object, int>> first = _ => i;
            Expression<Func<object, int>> second = _ => j;
            Expression<Func<int, int, int>> combineExpr = (x, y) => x + y;

            // act
            var result = ExpressionExtensions.Combine(first, second, combineExpr).Compile()(null);

            // assert
            result.Should().Be(i + j);
        }

        [Theory, AutoData]
        public void CombineUsesGivenParameterWhenDifferentParameterNames(int i)
        {
            // arrange
            Expression<Func<int, int>> first = x => x;
            Expression<Func<int, int>> second = y => y;
            Expression<Func<int, int, int>> combineExpr = (x, y) => x + y;

            // act
            var result = ExpressionExtensions.Combine(first, second, combineExpr).Compile()(i);

            // assert
            result.Should().Be(i + i);
        }

        [Theory, AutoData]
        public void CombineUsesGivenParameterWhenSameParameterNames(int i)
        {
            // arrange
            Expression<Func<int, int>> first = x => x;
            Expression<Func<int, int>> second = x => x;
            Expression<Func<int, int, int>> combineExpr = (x, y) => x + y;

            // act
            var result = ExpressionExtensions.Combine(first, second, combineExpr).Compile()(i);

            // assert
            result.Should().Be(i + i);
        }

        [Theory, AutoData]
        public void CombineAppliesParametersInCorrectOrder(int i, int j)
        {
            // arrange
            Expression<Func<object, int>> first = _ => i;
            Expression<Func<object, int>> second = _ => j;
            Expression<Func<int, int, int>> combineExpr = (x, y) => x;

            // act
            var result = ExpressionExtensions.Combine(first, second, combineExpr).Compile()(null);

            // assert
            result.Should().Be(i).And.NotBe(j);
        }

        [Theory, AutoData]
        public void ComposeReturnsResultOfSecondFunction(int i, int j)
        {
            // arrange
            Expression<Func<object, int>> first = _ => i;
            Expression<Func<int, int>> second = _ => j;

            // act
            var result = ExpressionExtensions.Compose(first, second).Compile()(null);

            // assert
            result.Should().Be(j).And.NotBe(i);
        }

        [Theory, AutoData]
        public void ComposePassesFirstResultToSecondFunction(int i)
        {
            // arrange
            Expression<Func<object, int>> first = _ => i;
            Expression<Func<int, int>> second = x => x;

            // act
            var result = ExpressionExtensions.Compose(first, second).Compile()(null);

            // assert
            result.Should().Be(i);
        }

        [Theory, AutoData]
        public void ComposePassesParameterToFirstFunction(int i)
        {
            // arrange
            Expression<Func<int, int>> first = x => x;
            Expression<Func<int, int>> second = x => x;

            // act
            var result = ExpressionExtensions.Compose(first, second).Compile()(i);

            // assert
            result.Should().Be(i);
        }

        [Theory, AutoData]
        public void CoalesceReturnsFunctionResultWhenNotNull(int i, int j)
        {
            // arrange
            Expression<Func<object, int?>> expr = _ => i;

            // act
            var result = ExpressionExtensions.Coalesce(expr, j).Compile()(null);

            // assert
            result.Should().Be(i).And.NotBe(j);
        }

        [Theory, AutoData]
        public void CoalesceReturnsGivenValueWhenNull(int j)
        {
            // arrange
            Expression<Func<object, int?>> expr = _ => null;

            // act
            var result = ExpressionExtensions.Coalesce(expr, j).Compile()(null);

            // assert
            result.Should().Be(j);
        }

        [Theory, AutoData]
        public void CoalesceReturnsDefaultValueWhenNullAndNoDefaultGiven()
        {
            // arrange
            Expression<Func<object, int?>> expr = _ => null;

            // act
            var result = ExpressionExtensions.Coalesce(expr).Compile()(null);

            // assert
            result.Should().Be(default(int));
        }
    }
}
