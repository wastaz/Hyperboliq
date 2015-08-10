using Xunit;
using Hyperboliq.Tests.Model;
using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using Direction = Hyperboliq.Domain.AST.Direction;
using NullsOrdering = Hyperboliq.Domain.AST.NullsOrdering;

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
                S.SelectNode(
                    S.Select(S.Star<Person>()),
                    S.From<Person>(),
                    orderBy: S.OrderBy(S.OrderClause(S.Col<Person>("Age"), Direction.Ascending, NullsOrdering.NullsFirst)));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal("SELECT PersonRef.* FROM Person PersonRef ORDER BY PersonRef.Age ASC NULLS FIRST", result);
        }

        [Fact]
        public void ItShouldBePossibleToSpecifyNullsLastOrdering()
        {
            var stream =
                S.SelectNode(
                     S.Select(S.Star<Person>()),
                    S.From<Person>(),
                    orderBy:
                        S.OrderBy(
                            S.OrderClause(S.Col<Person>("Age"), Direction.Ascending, NullsOrdering.NullsLast)
                        ));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal("SELECT PersonRef.* FROM Person PersonRef ORDER BY PersonRef.Age ASC NULLS LAST", result);
        }

        [Fact]
        public void ItShouldBePossibleToSpecifyDifferentNullsFirstOrLastOrderingOnEachColumn()
        {
            var stream =
                S.SelectNode(
                     S.Select(S.Star<Person>()),
                    S.From<Person>(),
                    orderBy:
                        S.OrderBy(
                            S.OrderClause(S.Col<Person>("Age"), Direction.Ascending, NullsOrdering.NullsLast),
                            S.OrderClause(S.Col<Person>("Name"), Direction.Descending, NullsOrdering.NullsFirst)
                        ));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal("SELECT PersonRef.* FROM Person PersonRef ORDER BY PersonRef.Age ASC NULLS LAST, PersonRef.Name DESC NULLS FIRST", result);
        }
    }
}
