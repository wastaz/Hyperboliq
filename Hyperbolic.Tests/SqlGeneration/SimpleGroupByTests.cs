using Xunit;
using Hyperboliq.Tests.Model;
using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using AggregateType = Hyperboliq.Domain.Stream.AggregateType;
using BinaryOperation = Hyperboliq.Domain.Stream.BinaryOperation;
using JoinType = Hyperboliq.Domain.Stream.JoinType;
using S = Hyperboliq.Tests.SqlStreamExtensions;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "GroupBy")]
    public class SqlGeneration_SimpleGroupByTests
    {
        [Fact]
        public void ItShouldBePossibleToGroupByASingleColumn()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Col<Person>("Name"), S.Aggregate(AggregateType.Max, S.Col<Person>("Age"))),
                    S.From<Person>(),
                    groupBy: S.GroupBy(S.Col<Person>("Name")));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.Name, MAX(PersonRef.Age) FROM Person PersonRef GROUP BY PersonRef.Name", result);
        }

        [Fact]
        public void ItShouldBePossibleToGroupByMultipleColumns()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Col<Person>("Name"), S.Col<Person>("LivesAtHouseId"), S.Aggregate(AggregateType.Min, S.Col<Person>("Age"))),
                    S.From<Person>(),
                    groupBy: S.GroupBy(S.Col<Person>("Name"), S.Col<Person>("LivesAtHouseId")));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(
                @"SELECT PersonRef.Name, PersonRef.LivesAtHouseId, MIN(PersonRef.Age) FROM Person PersonRef " +
                "GROUP BY PersonRef.Name, PersonRef.LivesAtHouseId",
                result);
        }

        [Fact]
        public void ItShouldBePossibleToGroupByColumnsFromSeveralTables()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Col<Person>("Age"), S.Col<Car>("Brand"), S.Aggregate(AggregateType.Count, S.Null())),
                    S.From<Person>(
                        S.Join<Person, Car>(
                            JoinType.InnerJoin,
                            S.BinExp(S.Col<Person>("Id"), BinaryOperation.Equal, S.Col<Car>("DriverId")))),
                    groupBy: S.GroupBy(S.Col<Person>("Age"), S.Col<Car>("Brand")));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal(
                "SELECT PersonRef.Age, CarRef.Brand, COUNT(*) " +
                "FROM Person PersonRef " +
                "INNER JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId " +
                "GROUP BY PersonRef.Age, CarRef.Brand",
                result);
        }

        [Fact]
        public void ItShouldBePossibleToUseASingleHavingExpression()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Col<Person>("Name"), S.Aggregate(AggregateType.Avg, S.Col<Person>("Age"))),
                    S.From<Person>(),
                    groupBy: 
                        S.GroupBy(
                            new[] { S.Col<Person>("Name") },
                            S.And(S.BinExp(S.Aggregate(AggregateType.Avg, S.Col<Person>("Age")), BinaryOperation.GreaterThan, S.Const(42)))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(
                "SELECT PersonRef.Name, AVG(PersonRef.Age) " +
                "FROM Person PersonRef " +
                "GROUP BY PersonRef.Name " +
                "HAVING AVG(PersonRef.Age) > 42",
                result);
        }

        [Fact]
        public void ItShouldBePossibleToUseMultipleHavingExpressions()
        {
            var stream =
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"),
                        S.Aggregate(AggregateType.Avg, S.Col<Person>("Age")),
                        S.Col<Car>("Brand"),
                        S.Aggregate(AggregateType.Min, S.Col<Car>("Age"))),
                    S.From<Person>(
                        S.Join<Person, Car>(JoinType.InnerJoin, S.BinExp(S.Col<Person>("Id"), BinaryOperation.Equal, S.Col<Car>("DriverId")))),
                    groupBy: 
                        S.GroupBy(
                            new[] { S.Col<Person>("Name"), S.Col<Car>("Brand"), },
                            S.And(S.BinExp(S.Aggregate(AggregateType.Avg, S.Col<Person>("Age")), BinaryOperation.GreaterThan, S.Const(42))),
                            S.And(S.BinExp(S.Aggregate(AggregateType.Min, S.Col<Car>("Age")), BinaryOperation.GreaterThan, S.Const(2)))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(
                "SELECT PersonRef.Name, AVG(PersonRef.Age), CarRef.Brand, MIN(CarRef.Age) " +
                "FROM Person PersonRef " +
                "INNER JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId " +
                "GROUP BY PersonRef.Name, CarRef.Brand " +
                "HAVING AVG(PersonRef.Age) > 42 " +
                "AND MIN(CarRef.Age) > 2",
                result);
        }
    }
}
