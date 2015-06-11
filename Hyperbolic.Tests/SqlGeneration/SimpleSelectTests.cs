using Hyperboliq.Tests.Model;
using Xunit;
using Hyperboliq.Dialects;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Domain;

namespace Hyperboliq.Tests.SqlGeneration
{
    
    [Trait("SqlGeneration", "Select")]
    public class SqlGeneration_SimpleSelectTests
    {
        [Fact]
        public void ItShouldBePossibleToSqlifyASimpleSelect()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>());

            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.* FROM Person PersonRef", result);
        }

        [Fact]
        public void ItShouldBePossibleToSqlifyASelectWithManyColumns()
        {
            var stream =
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"),
                        S.Col<Person>("Age"),
                        S.Col<Person>("Id")),
                    S.From<Person>());

            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.Name, PersonRef.Age, PersonRef.Id FROM Person PersonRef", result);
        }
    }
    
}
