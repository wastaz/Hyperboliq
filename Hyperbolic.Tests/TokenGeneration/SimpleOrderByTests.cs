using Xunit;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Stream;
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
            var result = expr.ToSqlExpression();

            var expected =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    orderBy: OrderBy(OrderClause(Col<Person>("Age"), Direction.Ascending)));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToOrderDescendingByAColumn()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .OrderBy<Person>(p => p.Age, Direction.Descending);
            var result = expr.ToSqlExpression();

            var expected =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    orderBy: OrderBy(OrderClause(Col<Person>("Age"), Direction.Descending)));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToOrderBySeveralColumns()
        {
            var expr = Select.Star<Person>().Star<Car>()
                             .From<Person>()
                             .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId)
                             .OrderBy<Person>(p => p.Age, Direction.Ascending)
                             .ThenBy<Car>(c => c.Brand, Direction.Descending);
            var result = expr.ToSqlExpression();

            var expected =
                SelectNode(
                    Select(Col<Car>("*"), Col<Person>("*")),
                    From<Person>(
                        Join<Person, Car>(JoinType.InnerJoin, BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")))),
                    orderBy: OrderBy(
                        OrderClause(Col<Car>("Brand"), Direction.Descending),
                        OrderClause(Col<Person>("Age"), Direction.Ascending))
                    );

            Assert.Equal(expected, result);
        }
    }
}
