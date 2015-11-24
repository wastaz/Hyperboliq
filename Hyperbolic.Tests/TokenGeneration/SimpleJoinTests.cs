using Xunit;
using Hyperboliq.Tests.TokenGeneration;
using JoinType = Hyperboliq.Domain.AST.JoinType;

namespace Hyperboliq.Tests
{
    [Trait("TokenGeneration", "Joins")]
    public class TokenGeneration_SimpleJoinTests
    {
        [Fact]
        public void ItShouldBeAbleToPerformASimpleInnerJoin()
        {
            var expr =
                Select.Star<Person>()
                      .From<Person>()
                      .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId);

            var result = expr.ToSqlExpression();

            var expected = TokenGeneration_SimpleJoinTests_Results.expectedAst(JoinType.InnerJoin);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBeAbleToPerformASimpleLeftJoin()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .LeftJoin<Person, Car>((p, c) => p.Id == c.DriverId);

            var result = expr.ToSqlExpression();

            var expected = TokenGeneration_SimpleJoinTests_Results.expectedAst(JoinType.LeftJoin);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBeAbleToPerformASimpleRightJoin()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .RightJoin<Person, Car>((p, c) => p.Id == c.DriverId);

            var result = expr.ToSqlExpression();

            var expected = TokenGeneration_SimpleJoinTests_Results.expectedAst(JoinType.RightJoin);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBeAbleToPerformASimpleFullJoin()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .FullJoin<Person, Car>((p, c) => p.Id == c.DriverId);

            var result = expr.ToSqlExpression();

            var expected = TokenGeneration_SimpleJoinTests_Results.expectedAst(JoinType.FullJoin);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBeAbleToPerformMultipleJoins()
        {
            var expr = Select.Star<Person>().Star<Car>().Star<House>()
                             .From<House>()
                             .InnerJoin<House, Person>((h, p) => h.Id == p.LivesAtHouseId)
                             .LeftJoin<Person, Car>((p, c) => p.Id == c.DriverId);
            var result = expr.ToSqlExpression();

            Assert.Equal(TokenGeneration_SimpleJoinTests_Results.multipleJoinExpression, result);
        }
    }
}
