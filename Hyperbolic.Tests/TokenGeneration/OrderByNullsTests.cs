using Xunit;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Stream;

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
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Select(Col<Person>("*")),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.OrderBy),
                    Ord(Col<Person>("Age"), Direction.Ascending, NullsOrdering.NullsFirst)
                    );

            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBePossibleToSpecifyNullsLastOrdering()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .OrderBy<Person>(p => p.Age, Direction.Ascending, NullsOrdering.NullsLast);
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Select(Col<Person>("*")),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.OrderBy),
                    Ord(Col<Person>("Age"), Direction.Ascending, NullsOrdering.NullsLast)
                    );

            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBePossibleToSpecifyDifferentNullsFirstOrLastOrderingOnEachColumn()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .OrderBy<Person>(p => p.Age, Direction.Ascending, NullsOrdering.NullsLast)
                             .ThenBy<Person>(p => p.Name, Direction.Descending, NullsOrdering.NullsFirst);
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Select(Col<Person>("*")),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.OrderBy),
                    Ord(Col<Person>("Age"), Direction.Ascending, NullsOrdering.NullsLast),
                    Ord(Col<Person>("Name"), Direction.Descending, NullsOrdering.NullsFirst)
                    );

            result.ShouldEqual(expected);
        }
    }
}
