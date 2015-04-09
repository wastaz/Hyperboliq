using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.ExpressionParts;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.SqlGenerator;
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
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.OrderBy),
                    Ord(Col<Person>("Age"), Direction.Ascending, NullsOrdering.NullsFirst)
                    );
            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be("SELECT PersonRef.* FROM Person PersonRef ORDER BY PersonRef.Age ASC NULLS FIRST");
        }

        [Fact]
        public void ItShouldBePossibleToSpecifyNullsLastOrdering()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.OrderBy),
                    Ord(Col<Person>("Age"), Direction.Ascending, NullsOrdering.NullsLast)
                    );
            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be("SELECT PersonRef.* FROM Person PersonRef ORDER BY PersonRef.Age ASC NULLS LAST");
        }

        [Fact]
        public void ItShouldBePossibleToSpecifyDifferentNullsFirstOrLastOrderingOnEachColumn()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.OrderBy),
                    Ord(Col<Person>("Age"), Direction.Ascending, NullsOrdering.NullsLast),
                    Ord(Col<Person>("Name"), Direction.Descending, NullsOrdering.NullsFirst)
                    );
            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be("SELECT PersonRef.* FROM Person PersonRef ORDER BY PersonRef.Age ASC NULLS LAST, PersonRef.Name DESC NULLS FIRST");
        }
    }
}
