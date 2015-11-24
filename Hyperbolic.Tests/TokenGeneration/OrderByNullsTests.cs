using Xunit;
using Hyperboliq.Tests.TokenGeneration;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using Direction = Hyperboliq.Domain.AST.Direction;
using NullsOrdering = Hyperboliq.Domain.AST.NullsOrdering;

namespace Hyperboliq.Tests
{
    [Trait("TokenGeneration", "OrderBy")]
    public class TokenGeneration_OrderByNullsTests
    {
        // According to ANSI SQL the order by clause should support the keywords NULLS FIRST/NULLS LAST
        // after ASC/DESC in an ordering clause. However some DBMS'es does not implement this part of the standard...
        // But some do!

        [Fact]
        public void ItShouldBePossibleToSpecifyNullsFirstOrdering()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .OrderBy<Person>(p => p.Age, Direction.Ascending, NullsOrdering.NullsFirst);
            var result = expr.ToSqlExpression();
            var expected =
                TokenGeneration_SimpleOrderByTests_Results.GetOrderByNullsOrderingExpression(NullsOrdering.NullsFirst);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToSpecifyNullsLastOrdering()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .OrderBy<Person>(p => p.Age, Direction.Ascending, NullsOrdering.NullsLast);
            var result = expr.ToSqlExpression();
            var expected =
                TokenGeneration_SimpleOrderByTests_Results.GetOrderByNullsOrderingExpression(NullsOrdering.NullsLast);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToSpecifyDifferentNullsFirstOrLastOrderingOnEachColumn()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .OrderBy<Person>(p => p.Age, Direction.Ascending, NullsOrdering.NullsLast)
                             .ThenBy<Person>(p => p.Name, Direction.Descending, NullsOrdering.NullsFirst);
            var result = expr.ToSqlExpression();
            Assert.Equal(TokenGeneration_SimpleOrderByTests_Results.orderByWithDifferentNullsOrderingPerColumnExpression, result);
        }
    }
}
