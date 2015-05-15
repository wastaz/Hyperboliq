using Xunit;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.SqlGen;
using static Hyperboliq.Domain.Types;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "OrderBy")]
    public class SqlGeneration_OrderByNullsTests
    {
        // According to ANSI SQL the order by clause should support the keywords NULLS FIRST/NULLS LAST
        // after ASC/DESC in an ordering clause. However some DBMS'es does not implement this part of the standard...
        // But some do!

        [Fact]
        public void ItShouldBePossibleToSpecifyNullsFirstOrdering()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    orderBy: OrderBy(OrderClause(Col<Person>("Age"), Direction.Ascending, NullsOrdering.NullsFirst)));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal("SELECT PersonRef.* FROM Person PersonRef ORDER BY PersonRef.Age ASC NULLS FIRST", result);
        }

        [Fact]
        public void ItShouldBePossibleToSpecifyNullsLastOrdering()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    orderBy:
                        OrderBy(
                            OrderClause(Col<Person>("Age"), Direction.Ascending, NullsOrdering.NullsLast)
                        ));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal("SELECT PersonRef.* FROM Person PersonRef ORDER BY PersonRef.Age ASC NULLS LAST", result);
        }

        [Fact]
        public void ItShouldBePossibleToSpecifyDifferentNullsFirstOrLastOrderingOnEachColumn()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    orderBy:
                        OrderBy(
                            OrderClause(Col<Person>("Age"), Direction.Ascending, NullsOrdering.NullsLast),
                            OrderClause(Col<Person>("Name"), Direction.Descending, NullsOrdering.NullsFirst)
                        ));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal("SELECT PersonRef.* FROM Person PersonRef ORDER BY PersonRef.Age ASC NULLS LAST, PersonRef.Name DESC NULLS FIRST", result);
        }
    }
}
