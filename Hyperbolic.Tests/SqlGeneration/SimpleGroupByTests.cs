using Xunit;
using Hyperboliq.Tests.Model;
using Hyperboliq.Dialects;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.SqlGen;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "GroupBy")]
    public class SqlGeneration_SimpleGroupByTests
    {
        [Fact]
        public void ItShouldBePossibleToGroupByASingleColumn()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("Name"), Aggregate(AggregateType.Max, Col<Person>("Age"))),
                    From<Person>(),
                    groupBy: GroupBy(Col<Person>("Name")));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.Name, MAX(PersonRef.Age) FROM Person PersonRef GROUP BY PersonRef.Name", result);
        }

        [Fact]
        public void ItShouldBePossibleToGroupByMultipleColumns()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("Name"), Col<Person>("LivesAtHouseId"), Aggregate(AggregateType.Min, Col<Person>("Age"))),
                    From<Person>(),
                    groupBy: GroupBy(Col<Person>("Name"), Col<Person>("LivesAtHouseId")));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(
                @"SELECT PersonRef.Name, PersonRef.LivesAtHouseId, MIN(PersonRef.Age) FROM Person PersonRef " +
                "GROUP BY PersonRef.Name, PersonRef.LivesAtHouseId",
                result);
        }

        [Fact]
        public void ItShouldBePossibleToGroupByColumnsFromSeveralTables()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("Age"), Col<Car>("Brand"), Aggregate(AggregateType.Count, Null())),
                    From<Person>(
                        Join<Person, Car>(
                            JoinType.InnerJoin,
                            BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")))),
                    groupBy: GroupBy(Col<Person>("Age"), Col<Car>("Brand")));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);

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
                SelectNode(
                    Select(Col<Person>("Name"), Aggregate(AggregateType.Avg, Col<Person>("Age"))),
                    From<Person>(),
                    groupBy: 
                        GroupBy(
                            new[] { Col<Person>("Name") },
                            And(BinExp(Aggregate(AggregateType.Avg, Col<Person>("Age")), BinaryOperation.GreaterThan, Const(42)))));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
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
                SelectNode(
                    Select(
                        Col<Person>("Name"),
                        Aggregate(AggregateType.Avg, Col<Person>("Age")),
                        Col<Car>("Brand"),
                        Aggregate(AggregateType.Min, Col<Car>("Age"))),
                    From<Person>(
                        Join<Person, Car>(JoinType.InnerJoin, BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")))),
                    groupBy: 
                        GroupBy(
                            new[] { Col<Person>("Name"), Col<Car>("Brand"), },
                            And(BinExp(Aggregate(AggregateType.Avg, Col<Person>("Age")), BinaryOperation.GreaterThan, Const(42))),
                            And(BinExp(Aggregate(AggregateType.Min, Col<Car>("Age")), BinaryOperation.GreaterThan, Const(2)))));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
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
