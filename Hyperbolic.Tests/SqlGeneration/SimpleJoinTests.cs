using Xunit;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.SqlGen;
using static Hyperboliq.Tests.SqlStreamExtensions;


namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "Joins")]
    public class SqlGeneration_SimpleJoinTests
    {
        private SqlExpression GetExpectedJoinStream(JoinType joinKeyword)
        {
            return
                SelectNode(
                    Select(Col<Person>("*"), Col<Car>("*")),
                    From<Person>(
                        Join<Person, Car>(joinKeyword, BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")))
                        ));
        }

        [Fact]
        public void ItShouldBeAbleToSqlifyASimpleInnerJoin()
        {
            var stream = GetExpectedJoinStream(JoinType.InnerJoin);
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.*, CarRef.* FROM Person PersonRef INNER JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId", result);
        }

        [Fact]
        public void ItShouldBeAbleToSqlifyASimpleLeftJoin()
        {
            var stream = GetExpectedJoinStream(JoinType.LeftJoin);
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.*, CarRef.* FROM Person PersonRef LEFT JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId", result);
        }

        [Fact]
        public void ItShouldBeAbleToSqlifyASimpleRightJoin()
        {
            var stream = GetExpectedJoinStream(JoinType.RightJoin);
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.*, CarRef.* FROM Person PersonRef RIGHT JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId", result);
        }

        [Fact]
        public void ItShouldBeAbleToSqlifyASimpleFullJoin()
        {
            var stream = GetExpectedJoinStream(JoinType.FullJoin);
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.*, CarRef.* FROM Person PersonRef FULL JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId", result);
        }

        [Fact]
        public void ItShouldBeAbleToSqlifyMultipleTableJoins()
        {
            var stream =
               SelectNode(
                   Select(Col<Person>("*"), Col<Car>("*"), Col<House>("*")),
                   From<House>(
                       Join<House, Person>(JoinType.InnerJoin, BinExp(Col<House>("Id"), BinaryOperation.Equal, Col<Person>("LivesAtHouseId"))),
                       Join<Person, Car>(JoinType.LeftJoin, BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")))));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(
                @"SELECT PersonRef.*, CarRef.*, HouseRef.* " +
                "FROM House HouseRef " +
                "INNER JOIN Person PersonRef ON HouseRef.Id = PersonRef.LivesAtHouseId " +
                "LEFT JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId", result);
        }
    }
}
