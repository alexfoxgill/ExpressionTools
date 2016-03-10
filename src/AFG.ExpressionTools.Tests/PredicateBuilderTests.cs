using AFG.ExpressionTools.Utility;
using FluentAssertions;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AFG.ExpressionTools.Tests
{
    public class PredicateBuilderTests
    {
        [Fact]
        public void AndShortCircuitsWhenFirstIsTrueConstant()
        {
            // arrange
            Expression<Func<int, bool>> first = x => true;
            Expression<Func<int, bool>> second = x => x > 0;

            // act
            var result = PredicateBuilder.And(first, second);

            // assert
            result.Should().Be(second);
        }

        [Fact]
        public void AndShortCircuitsWhenSecondIsTrueConstant()
        {
            // arrange
            Expression<Func<int, bool>> first = x => x > 0;
            Expression<Func<int, bool>> second = x => true;

            // act
            var result = PredicateBuilder.And(first, second);

            // assert
            result.Should().Be(first);
        }

        [Fact]
        public void AndShortCircuitsWhenFirstIsFalseConstant()
        {
            // arrange
            Expression<Func<int, bool>> first = x => false;
            Expression<Func<int, bool>> second = x => x > 0;

            // act
            var result = PredicateBuilder.And(first, second);

            // assert
            result.Should().Match<Expression<Func<int, bool>>>(x => ExpressionUtility.IsConstant(x, false));
        }

        [Fact]
        public void AndShortCircuitsWhenSecondIsFalseConstant()
        {
            // arrange
            Expression<Func<int, bool>> first = x => x > 0;
            Expression<Func<int, bool>> second = x => false;

            // act
            var result = PredicateBuilder.And(first, second);

            // assert
            result.Should().Match<Expression<Func<int, bool>>>(x => ExpressionUtility.IsConstant(x, false));
        }

        [Theory, AutoData]
        public void AndCompilesToAndExpression(int i)
        {
            // arrange
            Expression<Func<int, bool>> first = x => x > 0;
            Expression<Func<int, bool>> second = x => x % 2 == 0;

            // act
            var result = PredicateBuilder.And(first, second).Compile()(i);

            // assert
            result.Should().Be(first.Compile()(i) && second.Compile()(i));
        }

        [Fact]
        public void OrShortCircuitsWhenFirstIsTrueConstant()
        {
            // arrange
            Expression<Func<int, bool>> first = x => true;
            Expression<Func<int, bool>> second = x => x > 0;

            // act
            var result = PredicateBuilder.Or(first, second);

            // assert
            result.Should().Match<Expression<Func<int, bool>>>(x => ExpressionUtility.IsConstant(x, true));
        }

        [Fact]
        public void OrShortCircuitsWhenSecondIsTrueConstant()
        {
            // arrange
            Expression<Func<int, bool>> first = x => x > 0;
            Expression<Func<int, bool>> second = x => true;

            // act
            var result = PredicateBuilder.Or(first, second);

            // assert
            result.Should().Match<Expression<Func<int, bool>>>(x => ExpressionUtility.IsConstant(x, true));
        }

        [Fact]
        public void OrShortCircuitsWhenFirstIsFalseConstant()
        {
            // arrange
            Expression<Func<int, bool>> first = x => false;
            Expression<Func<int, bool>> second = x => x > 0;

            // act
            var result = PredicateBuilder.Or(first, second);

            // assert
            result.Should().Be(second);
        }

        [Fact]
        public void OrShortCircuitsWhenSecondIsFalseConstant()
        {
            // arrange
            Expression<Func<int, bool>> first = x => x > 0;
            Expression<Func<int, bool>> second = x => false;

            // act
            var result = PredicateBuilder.Or(first, second);

            // assert
            result.Should().Be(first);
        }

        [Theory, AutoData]
        public void AndCompilesToOrExpression(int i)
        {
            // arrange
            Expression<Func<int, bool>> first = x => x > 0;
            Expression<Func<int, bool>> second = x => x % 2 == 0;

            // act
            var result = PredicateBuilder.Or(first, second).Compile()(i);

            // assert
            result.Should().Be(first.Compile()(i) || second.Compile()(i));
        }

        [Theory, AutoData]
        public void NotNegatesPredicate(int i)
        {
            // arrange
            Expression<Func<int, bool>> predicate = x => x % 2 == 0;

            // act
            var result = PredicateBuilder.Not(predicate).Compile()(i);

            // assert
            result.Should().Be(!predicate.Compile()(i));
        }

        [Fact]
        public void AllReturnsTrueForEmptyList()
        {
            // arrange
            var predicates = Enumerable.Empty<Expression<Func<int, bool>>>();

            // act
            var result = PredicateBuilder.All(predicates);

            // assert
            result.Should().Match<Expression<Func<int, bool>>>(x => ExpressionUtility.IsConstant(x, true));
        }

        [Theory, AutoData]
        public void AllReturnsTrueWhenAllTrue(int i, int j)
        {
            // arrange
            var predicates = new List<Expression<Func<int, bool>>>
            {
                x => x == i,
                x => x != j,
                x => true
            };

            // act
            var result = PredicateBuilder.All(predicates).Compile()(i);

            // assert
            result.Should().BeTrue();
        }

        [Theory, AutoData]
        public void AllReturnsFalseWhenOneFalse(int i, int j)
        {
            // arrange
            var predicates = new List<Expression<Func<int, bool>>>
            {
                x => x == i,
                x => x != j,
                x => false
            };

            // act
            var result = PredicateBuilder.All(predicates).Compile()(i);

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void AnyReturnsFalseForEmptyList()
        {
            // arrange
            var predicates = Enumerable.Empty<Expression<Func<int, bool>>>();

            // act
            var result = PredicateBuilder.Any(predicates);

            // assert
            result.Should().Match<Expression<Func<int, bool>>>(x => ExpressionUtility.IsConstant(x, false));
        }

        [Theory, AutoData]
        public void AnyReturnsFalseWhenAllFalse(int i, int j)
        {
            // arrange
            var predicates = new List<Expression<Func<int, bool>>>
            {
                x => x != i,
                x => x == j,
                x => false
            };

            // act
            var result = PredicateBuilder.Any(predicates).Compile()(i);

            // assert
            result.Should().BeFalse();
        }

        [Theory, AutoData]
        public void AllReturnsTrueWhenOneTrue(int i, int j)
        {
            // arrange
            var predicates = new List<Expression<Func<int, bool>>>
            {
                x => x != i,
                x => x == j,
                x => true
            };

            // act
            var result = PredicateBuilder.Any(predicates).Compile()(i);

            // assert
            result.Should().BeTrue();
        }
    }
}
