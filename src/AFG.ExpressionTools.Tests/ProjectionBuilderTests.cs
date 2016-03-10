using FluentAssertions;
using Ploeh.AutoFixture.Xunit2;
using System;
using Xunit;

namespace AFG.ExpressionTools.Tests
{
    public class ProjectionBuilderTests
    {
        public class Foo
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Theory, AutoData]
        public void BuilderCreatesExpressionUsingInitialExpression(int id)
        {
            // arrange
            var sut = Projection.Create<int, Foo>(i => new Foo { Id = i });

            // act
            var result = sut.GetExpression.Compile()(id);

            // assert
            result.Id.Should().Be(id);
        }

        [Theory, AutoData]
        public void BuilderJoinsIntermediateToProjection(int id)
        {
            // arrange
            var sut = Projection.From<int>().To(i => new Foo { Id = i });

            // act
            var result = sut.GetExpression.Compile()(id);

            // assert
            result.Id.Should().Be(id);
        }

        [Theory, AutoData]
        public void ConstructorThrowsIfExpressionNotConstruction(Foo foo)
        {
            // arrange
            Action action = () => Projection.Create<int, Foo>(_ => foo);

            // act & assert
            action.ShouldThrow<ArgumentException>();
        }

        [Theory, AutoData]
        public void ConstructorDoesNotThrowForEmptyMemberInitExpression()
        {
            // arrange
            Action action = () => Projection.Create<int, Foo>(_ => new Foo());

            // act & assert
            action.ShouldNotThrow<ArgumentException>();
        }

        [Theory, AutoData]
        public void BlankConstructorUsesNewExpression(int id)
        {
            // arrange
            var sut = Projection.Create<int, Foo>();

            // act
            var result = sut.GetExpression.Compile()(id);

            // assert
            result.Should().NotBeNull().And.BeOfType<Foo>();
        }

        [Theory, AutoData]
        public void WithSetsNamedMemberUsingNewExpression(int id)
        {
            // arrange
            var sut = Projection.Create<int, Foo>()
                .With(x => x.Id, i => i);

            // act
            var result = sut.GetExpression.Compile()(id);

            // assert
            result.Id.Should().Be(id);
        }

        [Theory, AutoData]
        public void WithSetsNamedMemberUsingMemberInitExpression(int id, string name)
        {
            // arrange
            var sut = Projection.Create<int, Foo>(i => new Foo { Id = i })
                .With(x => x.Name, _ => name);

            // act
            var result = sut.GetExpression.Compile()(id);

            // assert
            result.Name.Should().Be(name);
        }

        [Theory, AutoData]
        public void WithDoesNotOverwriteExistingMemberInitExpression(int id, string name)
        {
            // arrange
            var sut = Projection.Create<int, Foo>(i => new Foo { Id = i })
                .With(x => x.Name, _ => name);

            // act
            var result = sut.GetExpression.Compile()(id);

            // assert
            result.Id.Should().Be(id);
        }
    }
}
