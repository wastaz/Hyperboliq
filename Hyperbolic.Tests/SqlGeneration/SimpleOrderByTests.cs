using System;
using Xunit;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Stream;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.SqlGenerator;
using FluentAssertions;

namespace Hyperboliq.Tests.SqlGeneration
{
    /*
    [Trait("SqlGeneration", "OrderBy")]
    public class SqlGeneration_SimpleOrderByTests
    {
        [Fact]
        public void ItShouldBePossibleToOrderAscendingByAColumn()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.OrderBy),
                    Ord(Col<Person>("Age"), Direction.Ascending)
                    );
            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.* FROM Person PersonRef ORDER BY PersonRef.Age ASC");
        }

        [Fact]
        public void ItShouldBePossibleToOrderDescendingByAColumn()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.OrderBy),
                    Ord(Col<Person>("Age"), Direction.Descending)
                    );
            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.* FROM Person PersonRef ORDER BY PersonRef.Age DESC");
        }

        [Fact]
        public void ItShouldBePossibleToOrderBySeveralColumns()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Col<Car>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.NewJoin(JoinType.InnerJoin)),
                    Tbl<Car>(),
                    Kw(KeywordNode.On),
                    BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")),
                    Kw(KeywordNode.OrderBy),
                    Ord(Col<Person>("Age"), Direction.Ascending),
                    Ord(Col<Car>("Brand"), Direction.Descending)
                    );
            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be(
                @"SELECT PersonRef.*, CarRef.* FROM Person PersonRef " +
                "INNER JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId " +
                "ORDER BY PersonRef.Age ASC, CarRef.Brand DESC");
        }
    }*/
}
