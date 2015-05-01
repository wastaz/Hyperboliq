﻿using Xunit;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Stream;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Domain.SqlGenerator;
using FluentAssertions;
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
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("Name"),
                    Col<Person>("Age"),
                    Col<Person>("Id"),
                    Kw(KeywordNode.From),
                    Tbl<Person>());

            var result = SqlifySeq(SqlServer.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.[Name], PersonRef.[Age], PersonRef.[Id] FROM Person PersonRef");
        }
    }
}