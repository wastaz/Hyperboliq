using System.Collections.Generic;
using Xunit;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.Stream;

namespace Hyperboliq.Tests
{
    [Trait("TokenGeneration", "Joins")]
    public class TokenGeneration_SimpleJoinTests
    {
        private SqlExpression GetExpectedStream(JoinType joinKeyword)
        {
            return SelectNode(
                Select(Col<Car>("*"), Col<Person>("*")),
                From<Person>(
                    Join<Person, Car>(joinKeyword, BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")))));
        }

        [Fact]
        public void ItShouldBeAbleToPerformASimpleInnerJoin()
        {
            var expr =
                Select.Star<Person>().Star<Car>()
                      .From<Person>()
                      .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId);

            var result = expr.ToSqlExpression();

            var expected = GetExpectedStream(JoinType.InnerJoin);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBeAbleToPerformASimpleLeftJoin()
        {
            var expr = Select.Star<Person>().Star<Car>()
                             .From<Person>()
                             .LeftJoin<Person, Car>((p, c) => p.Id == c.DriverId);

            var result = expr.ToSqlExpression();

            var expected = GetExpectedStream(JoinType.LeftJoin);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBeAbleToPerformASimpleRightJoin()
        {
            var expr = Select.Star<Person>().Star<Car>()
                             .From<Person>()
                             .RightJoin<Person, Car>((p, c) => p.Id == c.DriverId);

            var result = expr.ToSqlExpression();

            var expected = GetExpectedStream(JoinType.RightJoin);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBeAbleToPerformASimpleFullJoin()
        {
            var expr = Select.Star<Person>().Star<Car>()
                             .From<Person>()
                             .FullJoin<Person, Car>((p, c) => p.Id == c.DriverId);

            var result = expr.ToSqlExpression();

            var expected = GetExpectedStream(JoinType.FullJoin);
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

            var expected =
                SelectNode(
                    Select(Col<House>("*"), Col<Car>("*"), Col<Person>("*")),
                    From<House>(
                        Join<Person, Car>(JoinType.LeftJoin, BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId"))),
                        Join<House, Person>(JoinType.InnerJoin, BinExp(Col<House>("Id"), BinaryOperation.Equal, Col<Person>("LivesAtHouseId")))));

            Assert.Equal(expected, result);
        }
    }
}
