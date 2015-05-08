using Xunit;
using Hyperboliq.FluentApi;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Stream;

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
                InsertNode(
                    InsHead<Person>("Age", "Id", "LivesAtHouseId", "Name", "ParentId"),
                    InsVal(
                        InsConst(42),
                        InsConst(2),
                        InsConst(5),
                        InsConst("'Kalle'"),
                        InsConst(0))
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
                InsertNode(
                    InsHead<Person>("Name", "Age"),
                    InsVal(
                        InsConst("'Kalle'"),
                        InsConst(42)));

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
                InsertNode(
                    InsHead<Person>("Age", "Id", "LivesAtHouseId", "Name", "ParentId"),
                    InsVal(
                        InsConst(12),
                        InsConst(3),
                        InsConst(3),
                        InsConst("'Pelle'"),
                        InsConst(2)
                        ),
                    InsVal(
                        InsConst(42),
                        InsConst(2),
                        InsConst(5),
                        InsConst("'Kalle'"),
                        InsConst(0))
                    );

            Assert.Equal(expected, result);
        }
    }
}
