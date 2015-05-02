using System;
using Xunit;
using FluentAssertions;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.ExpressionParts;
using static Hyperboliq.Domain.Types;

namespace Hyperboliq.Tests
{
    [Trait("TokenGeneration", "OrderBy")]
    public class TokenGeneration_SimpleOrderByTests
    {
        [Fact]
        public void ItShouldBePossibleToOrderAscendingByAColumn()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .OrderBy<Person>(p => p.Age, Direction.Ascending);
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Select(Col<Person>("*")),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    OrderBy(
                        OrderClause(Col<Person>("Age"), Direction.Ascending))
                    );

            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBePossibleToOrderDescendingByAColumn()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .OrderBy<Person>(p => p.Age, Direction.Descending);
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Select(Col<Person>("*")),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    OrderBy(
                        OrderClause(Col<Person>("Age"), Direction.Descending))
                    );

            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBePossibleToOrderBySeveralColumns()
        {
            var expr = Select.Star<Person>().Star<Car>()
                             .From<Person>()
                             .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId)
                             .OrderBy<Person>(p => p.Age, Direction.Ascending)
                             .ThenBy<Car>(c => c.Brand, Direction.Descending);
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Select(Col<Car>("*"), Col<Person>("*")),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.NewJoin(JoinType.InnerJoin)),
                    Tbl<Car>(),
                    Kw(KeywordNode.On),
                    BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")),
                    OrderBy(
                        OrderClause(Col<Car>("Brand"), Direction.Descending),
                        OrderClause(Col<Person>("Age"), Direction.Ascending))
                    );

            result.ShouldEqual(expected);
        }
    }
}
