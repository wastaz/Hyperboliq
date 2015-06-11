using Xunit;
using Hyperboliq.Tests.Model;
using Hyperboliq.Domain;
using Hyperboliq.Dialects;
using Hyperboliq.Tests;
using S = Hyperboliq.Tests.SqlStreamExtensions;

namespace Hyperboliq.Tests.Sqllite
{
    [Trait("Sqlite", "Quoting")]
    public class SqlLite_ColumnQuotingTests
    {
        [Fact]
        public void ItShouldProperlyQuoteColumnNames()
        {
            var stream =
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"),
                        S.Col<Person>("Age"),
                        S.Col<Person>("Id")),
                    S.From<Person>());

            var result = SqlGen.SqlifyExpression(SqlLite.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.""Name"", PersonRef.""Age"", PersonRef.""Id"" FROM Person PersonRef", result);
        }
    }
}
