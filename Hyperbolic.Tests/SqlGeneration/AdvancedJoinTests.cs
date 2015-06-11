using Xunit;
using Hyperboliq.Tests.Model;
using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using JoinType = Hyperboliq.Domain.Stream.JoinType;
using BinaryOperation = Hyperboliq.Domain.Types.BinaryOperation;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "Joins")]
    public class SqlGeneration_AdvancedJoinTests
    {
        [Fact]
        public void ItShouldBePossibleToJoinATableToItself()
        {
            var child = Types.NamedTableReferenceFromType<Person>("child");
            var parent = Types.NamedTableReferenceFromType<Person>("parent");
            var stream =
                S.SelectNode(
                    S.Select(S.Col(child, "Name"), S.Col(parent, "Name")),
                    S.From(
                        child,
                        S.Join(child, parent, JoinType.InnerJoin, S.BinExp(S.Col(child, "ParentId"), BinaryOperation.Equal, S.Col(parent, "Id")))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(
                "SELECT child.Name, parent.Name FROM Person child "+
                "INNER JOIN Person parent ON child.ParentId = parent.Id", result);
        }
    }
}
