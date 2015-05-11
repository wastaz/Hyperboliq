using Xunit;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Stream;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.SqlGen;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "OrderBy")]
    public class SqlGeneration_SimpleOrderByTests
    {
        [Fact]
        public void ItShouldBePossibleToOrderAscendingByAColumn()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    orderBy: OrderBy(
                        OrderClause(Col<Person>("Age"), Direction.Ascending))
                    );
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.* FROM Person PersonRef ORDER BY PersonRef.Age ASC", result);
        }

        [Fact]
        public void ItShouldBePossibleToOrderDescendingByAColumn()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    orderBy: OrderBy(OrderClause(Col<Person>("Age"), Direction.Descending)));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.* FROM Person PersonRef ORDER BY PersonRef.Age DESC", result);
        }

        [Fact]
        public void ItShouldBePossibleToOrderBySeveralColumns()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("*"), Col<Car>("*")),
                    From<Person>(
                        Join<Person, Car>(JoinType.InnerJoin, BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")))),
                    orderBy: 
                        OrderBy(
                            OrderClause(Col<Person>("Age"), Direction.Ascending),
                            OrderClause(Col<Car>("Brand"), Direction.Descending)));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(
                @"SELECT PersonRef.*, CarRef.* FROM Person PersonRef " +
                "INNER JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId " +
                "ORDER BY PersonRef.Age ASC, CarRef.Brand DESC", result);
        }
    }
}
