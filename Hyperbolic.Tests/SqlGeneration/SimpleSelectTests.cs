using Xunit;
using static Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.SqlGen;
using FluentAssertions;
using static Hyperboliq.Domain.Types;

namespace Hyperboliq.Tests.SqlGeneration
{
    /*
    [Trait("SqlGeneration", "Select")]
    public class SqlGeneration_SimpleSelectTests
    {
        [Fact]
        public void ItShouldBePossibleToSqlifyASimpleSelect()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>());

            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.* FROM Person PersonRef");
        }

        [Fact]
        public void ItShouldBePossibleToSqlifyASelectWithManyColumns()
        {
            var stream =
                SelectNode(
                    Select(
                        Col<Person>("Name"),
                        Col<Person>("Age"),
                        Col<Person>("Id")),
                    From<Person>());

            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.Name, PersonRef.Age, PersonRef.Id FROM Person PersonRef");
        }
    }
    */
}
