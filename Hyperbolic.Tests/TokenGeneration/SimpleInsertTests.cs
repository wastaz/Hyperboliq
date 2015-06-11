using Xunit;
using Hyperboliq.Tests.Model;
using S = Hyperboliq.Tests.SqlStreamExtensions;

namespace Hyperboliq.Tests.TokenGeneration
{
    [Trait("TokenGeneration", "Insert")]
    public class TokenGeneration_SimpleInsertTests
    {
        [Fact]
        public void ItShouldBePossibleToDoASimpleInsert()
        {
            var val = new Person { Id = 2, Name = "Kalle", Age = 42, LivesAtHouseId = 5, ParentId = 0 };
            var expr = Insert.Into<Person>()
                             .AllColumns
                             .Value(val);
            var result = expr.ToSqlExpression();

            var expected = 
                S.InsertNode(
                    S.InsHead<Person>("Age", "Id", "LivesAtHouseId", "Name", "ParentId"),
                    S.InsVal(
                        S.InsConst(42),
                        S.InsConst(2),
                        S.InsConst(5),
                        S.InsConst("'Kalle'"),
                        S.InsConst(0))
                );

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToSpecifyColumnsForAnInsert()
        {
            var val = new Person { Id = 2, Name = "Kalle", Age = 42, LivesAtHouseId = 5, ParentId = 0 };
            var expr = Insert.Into<Person>()
                             .Columns(p => new { p.Name, p.Age })
                             .Value(val);
            var result = expr.ToSqlExpression();

            var expected = 
                S.InsertNode(
                    S.InsHead<Person>("Name", "Age"),
                    S.InsVal(
                        S.InsConst("'Kalle'"),
                        S.InsConst(42)));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToInsertMultipleValuesInOneStatement()
        {
            var val1 = new Person { Id = 2, Name = "Kalle", Age = 42, LivesAtHouseId = 5, ParentId = 0 };
            var val2 = new Person { Id = 3, Name = "Pelle", Age = 12, LivesAtHouseId = 3, ParentId = 2 };
            var expr = Insert.Into<Person>()
                             .AllColumns
                             .Values(val1, val2);
            var result = expr.ToSqlExpression();

            var expected =
                S.InsertNode(
                    S.InsHead<Person>("Age", "Id", "LivesAtHouseId", "Name", "ParentId"),
                    S.InsVal(
                        S.InsConst(12),
                        S.InsConst(3),
                        S.InsConst(3),
                        S.InsConst("'Pelle'"),
                        S.InsConst(2)
                        ),
                    S.InsVal(
                        S.InsConst(42),
                        S.InsConst(2),
                        S.InsConst(5),
                        S.InsConst("'Kalle'"),
                        S.InsConst(0))
                    );

            Assert.Equal(expected, result);
        }
    }
}
