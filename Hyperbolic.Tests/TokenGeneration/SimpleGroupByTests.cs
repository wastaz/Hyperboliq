using Xunit;
using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using AggregateType = Hyperboliq.Domain.AST.AggregateType;
using BinaryOperation = Hyperboliq.Domain.AST.BinaryOperation;
using JoinType = Hyperboliq.Domain.AST.JoinType;
using ValueNode = Hyperboliq.Domain.AST.ValueNode;

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
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"), 
                        S.AliasedCol<Person>(
                            AggregateType.Max,
                            S.Col<Person>("Age"),
                            "MaxAge")),
                    S.From<Person>(),
                    groupBy: S.GroupBy(S.Col<Person>("Name"))
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
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"), 
                        S.Col<Person>("LivesAtHouseId"), 
                        S.AliasedCol<Person>(
                            AggregateType.Min,
                            S.Col<Person>("Age"),
                            "MinAge")),
                    S.From<Person>(),
                    groupBy: S.GroupBy(S.Col<Person>("Name"), S.Col<Person>("LivesAtHouseId"))
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
                S.SelectNode(
                    S.Select(
                        S.Col<Car>("Brand"), 
                        S.AliasedCol<Person>(
                            AggregateType.Count,
                            ValueNode.NullValue,
                            "NumberOfPersons"),
                        S.Col<Person>("Age")),
                    S.From<Person>(
                        S.Join<Person, Car>(JoinType.InnerJoin, S.BinExp(S.Col<Person>("Id"), BinaryOperation.Equal, S.Col<Car>("DriverId")))),
                    groupBy: S.GroupBy(S.Col<Person>("Age"), S.Col<Car>("Brand"))
                    );

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToGroupByColumnsFromMultipleTables()
        {
            var expr = Select.Column<Person>(p => new { p.Name, AverageAge = Sql.Avg(p.Age) })
                             .Column<Car>(c => new { c.Brand, MinAge = Sql.Min(c.Age) })
                             .From<Person>()
                             .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId)
                             .GroupBy<Person>(p => p.Name)
                             .ThenBy<Car>(c => c.Brand);
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(
                        S.Col<Car>("Brand"),
                        S.AliasedCol<Person>(
                            AggregateType.Min,
                            S.Col<Car>("Age"), 
                            "MinAge"),
                        S.Col<Person>("Name"),
                        S.AliasedCol<Person>(
                            AggregateType.Avg,
                            S.Col<Person>("Age"),
                            "AverageAge")),
                    S.From<Person>(
                        S.Join<Person, Car>(JoinType.InnerJoin, S.BinExp(S.Col<Person>("Id"), BinaryOperation.Equal, S.Col<Car>("DriverId")))),
                    groupBy: S.GroupBy(S.Col<Person>("Name"), S.Col<Car>("Brand"))
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
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"), 
                        S.AliasedCol<Person>(
                            AggregateType.Avg, 
                            S.Col<Person>("Age"),
                            "AverageAge")),
                    S.From<Person>(),
                    groupBy: S.GroupBy(
                        new[] { S.Col<Person>("Name") }, 
                        S.And(S.BinExp(S.Aggregate(AggregateType.Avg, S.Col<Person>("Age")), BinaryOperation.GreaterThan, S.Const(42))))
                    );

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToUseMultipleHavingExpressions()
        {
            var expr = Select.Column<Person>(p => new { p.Name, AverageAge = Sql.Avg(p.Age) })
                             .Column<Car>(c => new { c.Brand, MinAge = Sql.Min(c.Age) })
                             .From<Person>()
                             .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId)
                             .GroupBy<Person>(p => p.Name).ThenBy<Car>(c => c.Brand)
                             .Having<Person>(p => Sql.Avg(p.Age) > 42)
                             .And<Car>(c => Sql.Min(c.Age) > 2);
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(
                        S.Col<Car>("Brand"),
                        S.AliasedCol<Person>(
                            AggregateType.Min, 
                            S.Col<Car>("Age"),
                            "MinAge"),
                        S.Col<Person>("Name"),
                        S.AliasedCol<Person>(
                            AggregateType.Avg, 
                            S.Col<Person>("Age"),
                            "AverageAge")),
                    S.From<Person>(
                        S.Join<Person, Car>(JoinType.InnerJoin, S.BinExp(S.Col<Person>("Id"), BinaryOperation.Equal, S.Col<Car>("DriverId")))),
                    groupBy: S.GroupBy(
                        new[] { S.Col<Person>("Name"), S.Col<Car>("Brand") },
                        S.And(
                            S.BinExp(
                                S.Aggregate(AggregateType.Min, S.Col<Car>("Age")),
                                BinaryOperation.GreaterThan,
                                S.Const(2))),
                        S.And(
                            S.BinExp(
                                S.Aggregate(AggregateType.Avg, S.Col<Person>("Age")),
                                BinaryOperation.GreaterThan,
                                S.Const(42)))
                        ));

            Assert.Equal(expected, result);
        }
    }
}
