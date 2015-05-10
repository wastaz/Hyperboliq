using Xunit;
using FluentAssertions;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.SqlGenerator;


namespace Hyperboliq.Tests.SqlGeneration
{
    /*
    [Trait("SqlGeneration", "Joins")]
    public class SqlGeneration_AdvancedJoinTests
    {
        [Fact]
        public void ItShouldBePossibleToJoinATableToItself()
        {
            var child = NamedTableReferenceFromType<Person>("child");
            var parent = NamedTableReferenceFromType<Person>("parent");
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col(child, "Name"),
                    Col(parent, "Name"),
                    Kw(KeywordNode.From),
                    Tbl(child),
                    Kw(KeywordNode.NewJoin(JoinType.InnerJoin)),
                    Tbl(parent),
                    Kw(KeywordNode.On),
                    BinExp(Col(child, "ParentId"), BinaryOperation.Equal, Col(parent, "Id"))
                    );
            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be(
                "SELECT child.Name, parent.Name FROM Person child "+
                "INNER JOIN Person parent ON child.ParentId = parent.Id");
        }
    }*/
}
