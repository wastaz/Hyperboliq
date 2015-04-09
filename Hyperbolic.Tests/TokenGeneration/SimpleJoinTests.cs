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
        private IEnumerable<SqlNode> GetExpectedStream(JoinType joinKeyword)
        {
            return StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Col<Car>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.NewJoin(joinKeyword)),
                    Tbl<Car>(),
                    Kw(KeywordNode.On),
                    BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId"))
                    );
        }

        [Fact]
        public void ItShouldBeAbleToPerformASimpleInnerJoin()
        {
            var expr =
                Select.Star<Person>().Star<Car>()
                      .From<Person>()
                      .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId);

            var result = expr.ToSqlStream();

            var expected = GetExpectedStream(JoinType.InnerJoin);
            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBeAbleToPerformASimpleLeftJoin()
        {
            var expr = Select.Star<Person>().Star<Car>()
                             .From<Person>()
                             .LeftJoin<Person, Car>((p, c) => p.Id == c.DriverId);

            var result = expr.ToSqlStream();

            var expected = GetExpectedStream(JoinType.LeftJoin);
            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBeAbleToPerformASimpleRightJoin()
        {
            var expr = Select.Star<Person>().Star<Car>()
                             .From<Person>()
                             .RightJoin<Person, Car>((p, c) => p.Id == c.DriverId);

            var result = expr.ToSqlStream();

            var expected = GetExpectedStream(JoinType.RightJoin);
            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBeAbleToPerformASimpleFullJoin()
        {
            var expr = Select.Star<Person>().Star<Car>()
                             .From<Person>()
                             .FullJoin<Person, Car>((p, c) => p.Id == c.DriverId);

            var result = expr.ToSqlStream();

            var expected = GetExpectedStream(JoinType.FullJoin);
            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBeAbleToPerformMultipleJoins()
        {
            var expr = Select.Star<Person>().Star<Car>().Star<House>()
                             .From<House>()
                             .InnerJoin<House, Person>((h, p) => h.Id == p.LivesAtHouseId)
                             .LeftJoin<Person, Car>((p, c) => p.Id == c.DriverId);
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Col<Car>("*"),
                    Col<House>("*"),
                    Kw(KeywordNode.From),
                    Tbl<House>(),
                    Kw(KeywordNode.NewJoin(JoinType.InnerJoin)),
                    Tbl<Person>(),
                    Kw(KeywordNode.On),
                    BinExp(Col<House>("Id"), BinaryOperation.Equal, Col<Person>("LivesAtHouseId")),
                    Kw(KeywordNode.NewJoin(JoinType.LeftJoin)),
                    Tbl<Car>(),
                    Kw(KeywordNode.On),
                    BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId"))
                    );

            result.ShouldEqual(expected);
        }
    }
}
