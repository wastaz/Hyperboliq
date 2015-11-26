using NUnit.Framework;
using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;
using S = Hyperboliq.Tests.SqlStreamExtensions;

namespace Hyperboliq.Tests.SqlServer
{
    [TestFixture]
    public class SqlServer_ColumnQuotingTests
    {
        [Test]
        public void ItShouldProperlyQuoteColumnNames()
        {
            var stream =
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"),
                        S.Col<Person>("Age"),
                        S.Col<Person>("Id")),
                    S.From<Person>());

            var result = SqlGen.SqlifyExpression(Dialects.SqlServer.Dialect, stream);
            Assert.That(result, Is.EqualTo(@"SELECT PersonRef.[Name], PersonRef.[Age], PersonRef.[Id] FROM Person PersonRef"));
        }
    }
}
