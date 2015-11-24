using Xunit;
using Hyperboliq.Tests.TokenGeneration;
using Direction = Hyperboliq.Domain.AST.Direction;

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
            var expected = TokenGeneration_SimpleOrderByTests_Results.GetOrderByDirectionExpression(Direction.Ascending);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToOrderDescendingByAColumn()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .OrderBy<Person>(p => p.Age, Direction.Descending);
            var result = expr.ToSqlExpression();
            var expected = TokenGeneration_SimpleOrderByTests_Results.GetOrderByDirectionExpression(Direction.Descending);
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
            Assert.Equal(TokenGeneration_SimpleOrderByTests_Results.orderByMultipleColumnsExpression, result);
        }
    }
}
