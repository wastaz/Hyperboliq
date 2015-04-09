using System;
using Xunit;
using FluentAssertions;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.SqlGenerator;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "GroupBy")]
    public class SqlGeneration_SimpleGroupByTests
    {
        [Fact]
        public void ItShouldBePossibleToGroupByASingleColumn()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("Name"),
                    Aggregate(AggregateType.Max, Col<Person>("Age")),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.GroupBy),
                    Col<Person>("Name")
                    );
            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.Name, MAX(PersonRef.Age) FROM Person PersonRef GROUP BY PersonRef.Name");
        }

        [Fact]
        public void ItShouldBePossibleToGroupByMultipleColumns()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("Name"),
                    Col<Person>("LivesAtHouseId"),
                    Aggregate(AggregateType.Min, Col<Person>("Age")),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.GroupBy),
                    Col<Person>("Name"),
                    Col<Person>("LivesAtHouseId")
                    );
            var result = SqlifySeq(AnsiSql.Dialect, stream);

            result.Should().Be(
                @"SELECT PersonRef.Name, PersonRef.LivesAtHouseId, MIN(PersonRef.Age) FROM Person PersonRef " +
                "GROUP BY PersonRef.Name, PersonRef.LivesAtHouseId");
        }

        [Fact]
        public void ItShouldBePossibleToGroupByColumnsFromSeveralTables()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("Age"),
                    Col<Car>("Brand"),
                    Aggregate(AggregateType.Count),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.NewJoin(JoinType.InnerJoin)),
                    Tbl<Car>(),
                    Kw(KeywordNode.On),
                    BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")),
                    Kw(KeywordNode.GroupBy),
                    Col<Person>("Age"),
                    Col<Car>("Brand")
                    );
            var result = SqlifySeq(AnsiSql.Dialect, stream);

            result.Should().Be(
                "SELECT PersonRef.Age, CarRef.Brand, COUNT(*) " +
                "FROM Person PersonRef " +
                "INNER JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId " +
                "GROUP BY PersonRef.Age, CarRef.Brand");
        }

        [Fact]
        public void ItShouldBePossibleToUseASingleHavingExpression()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("Name"),
                    Aggregate(AggregateType.Avg, Col<Person>("Age")),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.GroupBy),
                    Col<Person>("Name"),
                    Kw(KeywordNode.Having),
                    BinExp(Aggregate(AggregateType.Avg, Col<Person>("Age")), BinaryOperation.GreaterThan, Const(42))
                    );
            var result = SqlifySeq(AnsiSql.Dialect, stream);

            result.Should().Be(
                "SELECT PersonRef.Name, AVG(PersonRef.Age) " +
                "FROM Person PersonRef " +
                "GROUP BY PersonRef.Name " +
                "HAVING AVG(PersonRef.Age) > 42");
        }

        [Fact]
        public void ItShouldBePossibleToUseMultipleHavingExpressions()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("Name"),
                    Aggregate(AggregateType.Avg, Col<Person>("Age")),
                    Col<Car>("Brand"),
                    Aggregate(AggregateType.Min, Col<Car>("Age")),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.NewJoin(JoinType.InnerJoin)),
                    Tbl<Car>(),
                    Kw(KeywordNode.On),
                    BinExp(
                        Col<Person>("Id"), 
                        BinaryOperation.Equal, 
                        Col<Car>("DriverId")),
                    Kw(KeywordNode.GroupBy),
                    Col<Person>("Name"),
                    Col<Car>("Brand"),
                    Kw(KeywordNode.Having),
                    BinExp(
                        Aggregate(AggregateType.Avg, Col<Person>("Age")),
                        BinaryOperation.GreaterThan, 
                        Const(42)),
                    Kw(KeywordNode.And),
                    BinExp(
                        Aggregate(AggregateType.Min, Col<Car>("Age")),
                        BinaryOperation.GreaterThan, 
                        Const(2))
                    );
            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be(
                "SELECT PersonRef.Name, AVG(PersonRef.Age), CarRef.Brand, MIN(CarRef.Age) " +
                "FROM Person PersonRef " +
                "INNER JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId " +
                "GROUP BY PersonRef.Name, CarRef.Brand " +
                "HAVING AVG(PersonRef.Age) > 42 " +
                "AND MIN(CarRef.Age) > 2");
        }
    }
}
