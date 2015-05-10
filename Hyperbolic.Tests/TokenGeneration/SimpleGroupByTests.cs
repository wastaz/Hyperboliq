using System;
using Xunit;
using FluentAssertions;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;
using Microsoft.FSharp.Collections;

namespace Hyperboliq.Tests
{
    [Trait("TokenGeneration", "GroupBy")]
    public class TokenGeneration_SimpleGroupByTests
    {
        [Fact]
        public void ItShouldBePossibleToGroupByASingleColumn()
        {
            var expr = Select.Column<Person>(p => new { p.Name, MaxAge = Sql.Max(p.Age) })
                             .From<Person>()
                             .GroupBy<Person>(p => p.Name);
            var result = expr.ToSqlExpression();

            var expected =
                SelectNode(
                    Select(Col<Person>("Name"), Aggregate(AggregateType.Max, Col<Person>("Age"))),
                    From<Person>(),
                    groupBy: GroupBy(Col<Person>("Name"))
                    );

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToGroupByMultipleColumns()
        {
            var expr = Select.Column<Person>(p => new { p.Name, p.LivesAtHouseId, MinAge = Sql.Min(p.Age) })
                             .From<Person>()
                             .GroupBy<Person>(p => new { p.Name, p.LivesAtHouseId });
            var result = expr.ToSqlExpression();

            var expected =
                SelectNode(
                    Select(Col<Person>("Name"), Col<Person>("LivesAtHouseId"), Aggregate(AggregateType.Min, Col<Person>("Age"))),
                    From<Person>(),
                    groupBy: GroupBy(Col<Person>("Name"), Col<Person>("LivesAtHouseId"))
                    );

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToGroupByColumnsFromSeveralTables()
        {
            var expr = Select.Column<Person>(p => new { p.Age })
                             .Column<Car>(c => new { c.Brand, NumberOfPersons = Sql.Count() })
                             .From<Person>()
                             .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId)
                             .GroupBy<Person>(p => p.Age).ThenBy<Car>(c => c.Brand);
            var result = expr.ToSqlExpression();

            var expected =
                SelectNode(
                    Select(Col<Car>("Brand"), Aggregate(AggregateType.Count, ValueNode.NewValueList(FSharpList<ValueNode>.Empty)), Col<Person>("Age")),
                    From<Person>(
                        Join<Person, Car>(JoinType.InnerJoin, BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")))),
                    groupBy: GroupBy(Col<Person>("Age"), Col<Car>("Brand"))
                    );

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToGroupByColumnsFromMultipleTables()
        {
            var expr = Select.Column<Person>(p => new { p.Name, AverageAge = Sql.Avg(p.Age) })
                             .Column<Car>(c => new { c.Brand, MaxAge = Sql.Min(c.Age) })
                             .From<Person>()
                             .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId)
                             .GroupBy<Person>(p => p.Name)
                             .ThenBy<Car>(c => c.Brand);
            var result = expr.ToSqlExpression();

            var expected =
                SelectNode(
                    Select(
                        Col<Car>("Brand"),
                        Aggregate(AggregateType.Min, Col<Car>("Age")),
                        Col<Person>("Name"),
                        Aggregate(AggregateType.Avg, Col<Person>("Age"))),
                    From<Person>(
                        Join<Person, Car>(JoinType.InnerJoin, BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")))),
                    groupBy: GroupBy(Col<Person>("Name"), Col<Car>("Brand"))
                    );

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToUseASingleHavingExpression()
        {
            var expr = Select.Column<Person>(p => new { p.Name, AverageAge = Sql.Avg(p.Age) })
                             .From<Person>()
                             .GroupBy<Person>(p => p.Name)
                             .Having<Person>(p => Sql.Avg(p.Age) > 42);
            var result = expr.ToSqlExpression();

            var expected =
                SelectNode(
                    Select(Col<Person>("Name"), Aggregate(AggregateType.Avg, Col<Person>("Age"))),
                    From<Person>(),
                    groupBy: GroupBy(
                        new[] { Col<Person>("Name") }, 
                        And(BinExp(Aggregate(AggregateType.Avg, Col<Person>("Age")), BinaryOperation.GreaterThan, Const(42))))
                    );

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToUseMultipleHavingExpressions()
        {
            var expr = Select.Column<Person>(p => new { p.Name, AverageAge = Sql.Avg(p.Age) })
                             .Column<Car>(c => new { c.Brand, MaxAge = Sql.Min(c.Age) })
                             .From<Person>()
                             .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId)
                             .GroupBy<Person>(p => p.Name).ThenBy<Car>(c => c.Brand)
                             .Having<Person>(p => Sql.Avg(p.Age) > 42)
                             .And<Car>(c => Sql.Min(c.Age) > 2);
            var result = expr.ToSqlExpression();

            var expected =
                SelectNode(
                    Select(
                        Col<Car>("Brand"),
                        Aggregate(AggregateType.Min, Col<Car>("Age")),
                        Col<Person>("Name"),
                        Aggregate(AggregateType.Avg, Col<Person>("Age"))),
                    From<Person>(
                        Join<Person, Car>(JoinType.InnerJoin, BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")))),
                    groupBy: GroupBy(
                        new[] { Col<Person>("Name"), Col<Car>("Brand") },
                        And(
                            BinExp(
                                Aggregate(AggregateType.Min, Col<Car>("Age")),
                                BinaryOperation.GreaterThan,
                                Const(2))),
                        And(
                            BinExp(
                                Aggregate(AggregateType.Avg, Col<Person>("Age")),
                                BinaryOperation.GreaterThan,
                                Const(42)))
                        ));

            Assert.Equal(expected, result);
        }
    }
}
