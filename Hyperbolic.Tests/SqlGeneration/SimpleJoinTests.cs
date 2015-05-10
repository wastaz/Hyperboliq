using Xunit;
using static Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.SqlGenerator;
using FluentAssertions;
using System.Collections.Generic;

namespace Hyperboliq.Tests.SqlGeneration
{
    /*
    [Trait("SqlGeneration", "Joins")]
    public class SqlGeneration_SimpleJoinTests
    {
        
        private IEnumerable<SqlNode> GetExpectedJoinStream(JoinType joinKeyword)
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
        public void ItShouldBeAbleToSqlifyASimpleInnerJoin()
        {
            var stream = GetExpectedJoinStream(JoinType.InnerJoin);
            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.*, CarRef.* FROM Person PersonRef INNER JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId");
        }

        [Fact]
        public void ItShouldBeAbleToSqlifyASimpleLeftJoin()
        {
            var stream = GetExpectedJoinStream(JoinType.LeftJoin);
            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.*, CarRef.* FROM Person PersonRef LEFT JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId");
        }

        [Fact]
        public void ItShouldBeAbleToSqlifyASimpleRightJoin()
        {
            var stream = GetExpectedJoinStream(JoinType.RightJoin);
            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.*, CarRef.* FROM Person PersonRef RIGHT JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId");
        }

        [Fact]
        public void ItShouldBeAbleToSqlifyASimpleFullJoin()
        {
            var stream = GetExpectedJoinStream(JoinType.FullJoin);
            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.*, CarRef.* FROM Person PersonRef FULL JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId");
        }

        [Fact]
        public void ItShouldBeAbleToSqlifyMultipleTableJoins()
        {
            var stream =
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
            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be(
                @"SELECT PersonRef.*, CarRef.*, HouseRef.* " +
                "FROM House HouseRef " +
                "INNER JOIN Person PersonRef ON HouseRef.Id = PersonRef.LivesAtHouseId " +
                "LEFT JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId");
        }
    }
    */
}
