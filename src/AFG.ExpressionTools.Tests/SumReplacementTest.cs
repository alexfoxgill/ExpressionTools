using FluentAssertions;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AFG.ExpressionTools.Tests
{
    public class SumReplacementTest
    {
        [Fact]
        public void SumReturnsZeroForEmptyListWhenReplacedByNullableVersionWithSelector()
        {
            // arrange
            Expression<Func<IEnumerable<int>, int>> getSum = xs => xs.Sum(x => x);
            var list = Enumerable.Empty<int>();

            // act
            var result = getSum.FixSumExpression().Compile()(list);

            // assert
            result.Should().Be(0);
        }

        [Fact]
        public void SumReturnsZeroForEmptyListWhenReplacedByNullableVersionWithNoSelector()
        {
            // arrange
            Expression<Func<IEnumerable<int>, int>> getSum = xs => xs.Sum();
            var list = Enumerable.Empty<int>();

            // act
            var result = getSum.FixSumExpression().Compile()(list);

            // assert
            result.Should().Be(0);
        }

        [Theory, AutoData]
        public void SumReturnsSumWhenReplacedByNullableVersionWithSelector(List<int> list)
        {
            // arrange
            Expression<Func<IEnumerable<int>, int>> getSum = xs => xs.Sum(x => x);

            // act
            var result = getSum.FixSumExpression().Compile()(list);

            // assert
            result.Should().Be(list.Sum());
        }

        [Theory, AutoData]
        public void SumReturnsSumWhenReplacedByNullableVersionWithNoSelector(List<int> list)
        {
            // arrange
            Expression<Func<IEnumerable<int>, int>> getSum = xs => xs.Sum();

            // act
            var result = getSum.FixSumExpression().Compile()(list);

            // assert
            result.Should().Be(list.Sum());
        }
    }
}
