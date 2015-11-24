using Xunit;
using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;
using S = Hyperboliq.Tests.SqlStreamExtensions;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "Insert")]
    public class SqlGeneration_SimpleInsertTests
    {
        [Fact]
        public void ItShouldBePossibleToDoASimpleInsert()
        {
            var stream =
                S.InsertNode(
                    S.InsHead<Person>("Age", "Id", "LivesAtHouseId", "Name", "ParentId"),
                    S.InsVal(
                        S.InsConst(42),
                        S.InsConst(2),
                        S.InsConst(5),
                        S.InsConst("'Kalle'"),
                        S.InsConst(0))
                    );

            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal("INSERT INTO Person (Age, Id, LivesAtHouseId, Name, ParentId) VALUES (42, 2, 5, 'Kalle', 0)", result);
        }

        [Fact]
        public void ItShouldBePossibleToSpecifyColumnsForAnInsert()
        {
            var stream = 
                S.InsertNode(
                    S.InsHead<Person>("Name", "Age"),
                    S.InsVal(
                        S.InsConst("'Kalle'"),
                        S.InsConst(42))
                    );
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal("INSERT INTO Person (Name, Age) VALUES ('Kalle', 42)", result);
        }

        [Fact]
        public void ItShouldBePossibleToInsertMultipleValuesInOneStatement()
        {
            var stream =
                S.InsertNode(
                    S.InsHead<Person>("Age", "Id", "LivesAtHouseId", "Name", "ParentId"),
                    S.InsVal(
                        S.InsConst(42),
                        S.InsConst(2),
                        S.InsConst(5),
                        S.InsConst("'Kalle'"),
                        S.InsConst(0)),
                    S.InsVal(
                        S.InsConst(12),
                        S.InsConst(3),
                        S.InsConst(3),
                        S.InsConst("'Pelle'"),
                        S.InsConst(2)
                        )
                    );
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal(
                "INSERT INTO Person (Age, Id, LivesAtHouseId, Name, ParentId) " +
                "VALUES (42, 2, 5, 'Kalle', 0), (12, 3, 3, 'Pelle', 2)",
                result);
        }
    }
}
