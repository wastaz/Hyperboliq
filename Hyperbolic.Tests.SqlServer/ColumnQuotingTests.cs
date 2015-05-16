using Xunit;
using static Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Domain.SqlGen;
using Hyperboliq.Dialects;

namespace Hyperboliq.Tests.Sqllite
{
    [Trait("SqlServer", "Quoting")]
    public class SqlServer_ColumnQuotingTests
    {
        [Fact]
        public void ItShouldProperlyQuoteColumnNames()
        {
            var stream =
                SelectNode(
                    Select(
                        Col<Person>("Name"),
                        Col<Person>("Age"),
                        Col<Person>("Id")),
                    From<Person>());

            var result = SqlifyExpression(SqlServer.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.[Name], PersonRef.[Age], PersonRef.[Id] FROM Person PersonRef", result);
        }
    }
}
