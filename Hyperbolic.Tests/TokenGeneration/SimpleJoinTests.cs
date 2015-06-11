using System.Collections.Generic;
using Xunit;
using Hyperboliq.Tests.Model;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.Types.BinaryOperation;
using JoinType = Hyperboliq.Domain.Stream.JoinType;
using SqlExpression = Hyperboliq.Domain.Stream.SqlExpression;

namespace Hyperboliq.Tests
{
    [Trait("TokenGeneration", "Joins")]
    public class TokenGeneration_SimpleJoinTests
    {
        private SqlExpression GetExpectedStream(JoinType joinKeyword)
        {
            return S.SelectNode(
                S.Select(S.Col<Car>("*"), S.Col<Person>("*")),
                S.From<Person>(
                    S.Join<Person, Car>(joinKeyword, S.BinExp(S.Col<Person>("Id"), BinaryOperation.Equal, S.Col<Car>("DriverId")))));
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
                S.SelectNode(
                    S.Select(S.Col<House>("*"), S.Col<Car>("*"), S.Col<Person>("*")),
                    S.From<House>(
                        S.Join<Person, Car>(JoinType.LeftJoin, S.BinExp(S.Col<Person>("Id"), BinaryOperation.Equal, S.Col<Car>("DriverId"))),
                        S.Join<House, Person>(JoinType.InnerJoin, S.BinExp(S.Col<House>("Id"), BinaryOperation.Equal, S.Col<Person>("LivesAtHouseId")))));

            Assert.Equal(expected, result);
        }
    }
}
