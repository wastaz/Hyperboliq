using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Hyperboliq.Domain;
using Hyperboliq.Dialects;
using Hyperboliq.Tests.Model;
using AggregateType = Hyperboliq.Domain.Stream.AggregateType;
using Direction = Hyperboliq.Domain.Stream.Direction;
using NullsOrdering = Hyperboliq.Domain.Stream.NullsOrdering;
using S = Hyperboliq.Tests.SqlStreamExtensions;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "SelectOver")]
    public class SqlGeneration_SelectPartitionsTests
    {
        [Fact]
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

            Assert.Equal(
                "SELECT PersonRef.Name, SUM(PersonRef.Age) OVER () FROM Person PersonRef", 
                result);
        }

        [Fact]
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

            Assert.Equal(
                "SELECT PersonRef.Name, MAX(PersonRef.Age) OVER (PARTITION BY PersonRef.Name) FROM Person PersonRef", 
                result);
        }

        [Fact]
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

            Assert.Equal(
                "SELECT PersonRef.Name, MAX(PersonRef.Age) OVER (PARTITION BY PersonRef.Name, PersonRef.LivesAtHouseId) FROM Person PersonRef", 
                result);
        }

        [Fact]
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

            Assert.Equal(
                "SELECT PersonRef.Name, SUM(PersonRef.Age) OVER (ORDER BY PersonRef.Age ASC) FROM Person PersonRef", 
                result);
        }

        [Fact]
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

            Assert.Equal(
                "SELECT PersonRef.Name, SUM(PersonRef.Age) OVER (ORDER BY PersonRef.Age ASC NULLS LAST, PersonRef.Name DESC) FROM Person PersonRef", 
                result);
        }

        [Fact]
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

            Assert.Equal(
                "SELECT PersonRef.Name, SUM(PersonRef.Age) OVER (PARTITION BY PersonRef.Name ORDER BY PersonRef.Age ASC) FROM Person PersonRef", 
                result);
        }
    }
}
