using Xunit;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using FluentAssertions;
using static Hyperboliq.Domain.SqlGenerator;
using static Hyperboliq.Domain.Types;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "Insert")]
    public class SqlGeneration_SimpleInsertTests
    {
        [Fact]
        public void ItShouldBePossibleToDoASimpleInsert()
        {
            var stream = InsertNode(
                InsHead<Person>("Age", "Id", "LivesAtHouseId", "Name", "ParentId"),
                InsVal(
                    InsConst(42),
                    InsConst(2),
                    InsConst(5),
                    InsConst("'Kalle'"),
                    InsConst(0))
                );

            var result = SqlifyExpression(AnsiSql.Dialect, stream);

            result.Should().Be("INSERT INTO Person (Age, Id, LivesAtHouseId, Name, ParentId) VALUES (42, 2, 5, 'Kalle', 0)");
        }

        [Fact]
        public void ItShouldBePossibleToSpecifyColumnsForAnInsert()
        {
            var stream = InsertNode(
                InsHead<Person>("Name", "Age"),
                InsVal(
                    InsConst("'Kalle'"),
                    InsConst(42))
                );
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            result.Should().Be("INSERT INTO Person (Name, Age) VALUES ('Kalle', 42)");
        }

        [Fact]
        public void ItShouldBePossibleToInsertMultipleValuesInOneStatement()
        {
            var stream =
                InsertNode(
                    InsHead<Person>("Age", "Id", "LivesAtHouseId", "Name", "ParentId"),
                    InsVal(
                        InsConst(42),
                        InsConst(2),
                        InsConst(5),
                        InsConst("'Kalle'"),
                        InsConst(0)),
                    InsVal(
                        InsConst(12),
                        InsConst(3),
                        InsConst(3),
                        InsConst("'Pelle'"),
                        InsConst(2)
                        )
                    );
            var result = SqlifyExpression(AnsiSql.Dialect, stream);

            result.Should().Be(
                "INSERT INTO Person (Age, Id, LivesAtHouseId, Name, ParentId) " +
                "VALUES (42, 2, 5, 'Kalle', 0), (12, 3, 3, 'Pelle', 2)");
        }
    }
}
