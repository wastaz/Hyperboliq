using NUnit.Framework;
using Hyperboliq.Tests.TokenGeneration;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using Direction = Hyperboliq.Domain.AST.Direction;
using NullsOrdering = Hyperboliq.Domain.AST.NullsOrdering;

namespace Hyperboliq.Tests
{
    [TestFixture]
    public class TokenGeneration_OrderByNullsTests
    {
        // According to ANSI SQL the order by clause should support the keywords NULLS FIRST/NULLS LAST
        // after ASC/DESC in an ordering clause. However some DBMS'es does not implement this part of the standard...
        // But some do!

        [Test]
        public void ItShouldBePossibleToSpecifyNullsFirstOrdering()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .OrderBy<Person>(p => p.Age, Direction.Ascending, NullsOrdering.NullsFirst);
            var result = expr.ToSqlExpression();
            var expected =
                TokenGeneration_SimpleOrderByTests_Results.GetOrderByNullsOrderingExpression(NullsOrdering.NullsFirst);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBePossibleToSpecifyNullsLastOrdering()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .OrderBy<Person>(p => p.Age, Direction.Ascending, NullsOrdering.NullsLast);
            var result = expr.ToSqlExpression();
            var expected =
                TokenGeneration_SimpleOrderByTests_Results.GetOrderByNullsOrderingExpression(NullsOrdering.NullsLast);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBePossibleToSpecifyDifferentNullsFirstOrLastOrderingOnEachColumn()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .OrderBy<Person>(p => p.Age, Direction.Ascending, NullsOrdering.NullsLast)
                             .ThenBy<Person>(p => p.Name, Direction.Descending, NullsOrdering.NullsFirst);
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleOrderByTests_Results.orderByWithDifferentNullsOrderingPerColumnExpression));
        }
    }
}
