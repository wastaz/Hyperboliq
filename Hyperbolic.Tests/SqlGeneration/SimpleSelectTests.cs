using Xunit;
using static Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.SqlGenerator;
using FluentAssertions;
using static Hyperboliq.Domain.Types;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "Select")]
    public class SqlGeneration_SimpleSelectTests
    {
        [Fact]
        public void ItShouldBePossibleToSqlifyASimpleSelect()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>());

            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.* FROM Person PersonRef");
        }

        [Fact]
        public void ItShouldBePossibleToSqlifyASelectWithManyColumns()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("Name"),
                    Col<Person>("Age"),
                    Col<Person>("Id"),
                    Kw(KeywordNode.From),
                    Tbl<Person>());

            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.Name, PersonRef.Age, PersonRef.Id FROM Person PersonRef");
        }
    }
}
