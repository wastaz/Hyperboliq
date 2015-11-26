using NUnit.Framework;
using Hyperboliq.Domain;
using Hyperboliq.Dialects;
using Hyperboliq.Tests.TokenGeneration;
using AggregateType = Hyperboliq.Domain.AST.AggregateType;
using Direction = Hyperboliq.Domain.AST.Direction;
using NullsOrdering = Hyperboliq.Domain.AST.NullsOrdering;
using S = Hyperboliq.Tests.SqlStreamExtensions;

namespace Hyperboliq.Tests.SqlGeneration
{
    [TestFixture]
    public class SqlGeneration_SelectPartitionsTests
    {
        [Test]
        public void ItShouldBePossibleToUseAnEmptyOverClause()
        {
            var expr =
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"),
                        S.WinCol(
                            AggregateType.Sum,
                            S.Col<Person>("Age"))),
                    S.From<Person>());
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, expr);

            Assert.That(result, Is.EqualTo("SELECT PersonRef.Name, SUM(PersonRef.Age) OVER () FROM Person PersonRef"));
        }

        [Test]
        public void ItShouldBePossibleToPartitionByAColumn()
        {
            var expr =
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"),
                        S.WinCol(
                            AggregateType.Max, 
                            S.Col<Person>("Age"), 
                            new[] { S.Col<Person>("Name") })),
                    S.From<Person>());
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, expr);

            Assert.That(result, Is.EqualTo("SELECT PersonRef.Name, MAX(PersonRef.Age) OVER (PARTITION BY PersonRef.Name) FROM Person PersonRef"));
        }

        [Test]
        public void ItShouldBePossibleToPartitionByMultipleColumns()
        {
            var expr =
                S.SelectNode(
                    S.Select(
                    S.Col<Person>("Name"),
                    S.WinCol(
                        AggregateType.Max,
                        S.Col<Person>("Age"),
                        partitionBy: new[] { S.Col<Person>("Name"), S.Col<Person>("LivesAtHouseId"), })),
                    S.From<Person>()
                );
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, expr);

            Assert.That(result, Is.EqualTo("SELECT PersonRef.Name, MAX(PersonRef.Age) OVER (PARTITION BY PersonRef.Name, PersonRef.LivesAtHouseId) FROM Person PersonRef"));
        }

        [Test]
        public void ItShouldBePossibleToOrderByAColumn()
        {
            var expr =
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"), 
                        S.WinCol(
                            AggregateType.Sum, 
                            S.Col<Person>("Age"), 
                            orderBy: new[] { S.OrderClause(S.Col<Person>("Age"), Direction.Ascending) })),
                    S.From<Person>());
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, expr);

            Assert.That(result, Is.EqualTo("SELECT PersonRef.Name, SUM(PersonRef.Age) OVER (ORDER BY PersonRef.Age ASC) FROM Person PersonRef"));
        }

        [Test]
        public void ItShouldBePossibleToOrderByMultipleColumns()
        {
            var expr =
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"),
                        S.WinCol(
                            AggregateType.Sum,
                            S.Col<Person>("Age"),
                            orderBy: new[] 
                            { 
                                S.OrderClause(S.Col<Person>("Age"), Direction.Ascending, NullsOrdering.NullsLast), 
                                S.OrderClause(S.Col<Person>("Name"), Direction.Descending), 
                            })),
                    S.From<Person>());
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, expr);

            Assert.That(result, Is.EqualTo("SELECT PersonRef.Name, SUM(PersonRef.Age) OVER (ORDER BY PersonRef.Age ASC NULLS LAST, PersonRef.Name DESC) FROM Person PersonRef"));
        }

        [Test]
        public void ItShouldBePossibleToBothPartitionAndOrderByAColumn()
        {
            var expr =
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"),
                        S.WinCol(
                            AggregateType.Sum,
                            S.Col<Person>("Age"),
                            partitionBy: new[] { S.Col<Person>("Name") },
                            orderBy: new[] { S.OrderClause(S.Col<Person>("Age"), Direction.Ascending) }
                            )),
                    S.From<Person>());
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, expr);

            Assert.That(result, Is.EqualTo("SELECT PersonRef.Name, SUM(PersonRef.Age) OVER (PARTITION BY PersonRef.Name ORDER BY PersonRef.Age ASC) FROM Person PersonRef"));
        }
    }
}
