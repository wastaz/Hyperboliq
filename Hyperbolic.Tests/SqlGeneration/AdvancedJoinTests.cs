using Xunit;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.SqlGen;


namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "Joins")]
    public class SqlGeneration_AdvancedJoinTests
    {
        [Fact]
        public void ItShouldBePossibleToJoinATableToItself()
        {
            var child = NamedTableReferenceFromType<Person>("child");
            var parent = NamedTableReferenceFromType<Person>("parent");
            var stream =
                SelectNode(
                    Select(Col(child, "Name"), Col(parent, "Name")),
                    From(
                        child,
                        Join(child, parent, JoinType.InnerJoin, BinExp(Col(child, "ParentId"), BinaryOperation.Equal, Col(parent, "Id")))));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(
                "SELECT child.Name, parent.Name FROM Person child "+
                "INNER JOIN Person parent ON child.ParentId = parent.Id", result);
        }
    }
}
