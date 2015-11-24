using Xunit;
using Hyperboliq.Domain;
using AggregateType = Hyperboliq.Domain.AST.AggregateType;
using Direction = Hyperboliq.Domain.AST.Direction;
using NullsOrdering = Hyperboliq.Domain.AST.NullsOrdering;
using S = Hyperboliq.Tests.SqlStreamExtensions;

namespace Hyperboliq.Tests.TokenGeneration
{
    [Trait("TokenGeneration", "SelectOver")]
    public class TokenGeneration_SelectPartitionsTests
    {
        [Fact]
        public void ItShouldBePossibleToUseAnEmptyOverClause()
        {
            var expr = Select.Column<Person>(p => p.Name)
                             .Column<Person>(p => Sql.Sum(p.Age), Over.Empty)
                             .From<Person>();
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"),
                        S.WinCol(
                            AggregateType.Sum,
                            S.Col<Person>("Age"))),
                    S.From<Person>());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToPartitionByAColumn()
        {
            var expr = Select.Column<Person>(p => p.Name)
                             .Column<Person>(p => Sql.Max(p.Age), Over.PartitionBy<Person>(p => p.Name))
                             .From<Person>();
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"),
                        S.WinCol(
                            AggregateType.Max, 
                            S.Col<Person>("Age"), 
                            new[] { S.Col<Person>("Name") })),
                    S.From<Person>());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToPartitionByMultipleColumns()
        {
            var expr = Select.Column<Person>(p => p.Name)
                             .Column<Person>(p => Sql.Max(p.Age), Over.PartitionBy<Person>(p => p.Name).ThenBy<Person>(p => p.LivesAtHouseId))
                             .From<Person>();
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(
                    S.Col<Person>("Name"),
                    S.WinCol(
                        AggregateType.Max,
                        S.Col<Person>("Age"),
                        partitionBy: new[] { S.Col<Person>("Name"), S.Col<Person>("LivesAtHouseId"), })),
                    S.From<Person>()
                );

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToOrderByAColumn()
        {
            var expr = Select.Column<Person>(p => p.Name)
                             .Column<Person>(p => Sql.Sum(p.Age), Over.OrderBy<Person>(p => p.Age))
                             .From<Person>();
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"), 
                        S.WinCol(
                            AggregateType.Sum, 
                            S.Col<Person>("Age"), 
                            orderBy: new[] { S.OrderClause(S.Col<Person>("Age"), Direction.Ascending) })),
                    S.From<Person>());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToOrderByMultipleColumns()
        {
            var expr = Select.Column<Person>(p => p.Name)
                             .Column<Person>(p => Sql.Sum(p.Age), Over.OrderBy<Person>(p => p.Age, Direction.Ascending, NullsOrdering.NullsLast).ThenBy<Person>(p => p.Name, Direction.Descending))
                             .From<Person>();
            var result = expr.ToSqlExpression();

            var expected =
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
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToBothPartitionAndOrderByAColumn()
        {
            var expr = Select.Column<Person>(p => p.Name)
                             .Column<Person>(
                                p => Sql.Sum(p.Age), 
                                Over.PartitionBy<Person>(p => p.Name).OrderBy<Person>(p => p.Age))
                             .From<Person>();
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"),
                        S.WinCol(
                            AggregateType.Sum,
                            S.Col<Person>("Age"),
                            partitionBy: new[] { S.Col<Person>("Name") },
                            orderBy: new[] { S.OrderClause(S.Col<Person>("Age"), Direction.Ascending) }
                            )),
                    S.From<Person>()
                );
            Assert.Equal(expected, result);
        }
    }
}
