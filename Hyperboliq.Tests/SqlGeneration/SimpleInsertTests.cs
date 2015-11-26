using NUnit.Framework;
using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;
using S = Hyperboliq.Tests.SqlStreamExtensions;

namespace Hyperboliq.Tests.SqlGeneration
{
    [TestFixture]
    public class SqlGeneration_SimpleInsertTests
    {
        [Test]
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

            Assert.That(
                result,
                Is.EqualTo("INSERT INTO Person (Age, Id, LivesAtHouseId, Name, ParentId) VALUES (42, 2, 5, 'Kalle', 0)"));
        }

        [Test]
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
            Assert.That(
                result,
                Is.EqualTo("INSERT INTO Person (Name, Age) VALUES ('Kalle', 42)"));
        }

        [Test]
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

            Assert.That(
                result,
                Is.EqualTo("INSERT INTO Person (Age, Id, LivesAtHouseId, Name, ParentId) " +
                           "VALUES (42, 2, 5, 'Kalle', 0), (12, 3, 3, 'Pelle', 2)"));
        }
    }
}
