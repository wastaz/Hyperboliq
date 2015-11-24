using Xunit;
using Hyperboliq.Domain;
using Direction = Hyperboliq.Domain.AST.Direction;
using NullsOrdering = Hyperboliq.Domain.AST.NullsOrdering;

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

            Assert.Equal(TokenGeneration_SelectPartitionTests_Results.emptyOverClauseExpression, result);
        }

        [Fact]
        public void ItShouldBePossibleToPartitionByAColumn()
        {
            var expr = Select.Column<Person>(p => p.Name)
                             .Column<Person>(p => Sql.Max(p.Age), Over.PartitionBy<Person>(p => p.Name))
                             .From<Person>();
            var result = expr.ToSqlExpression();
            
            Assert.Equal(TokenGeneration_SelectPartitionTests_Results.partitionByColumnExpression, result);
        }

        [Fact]
        public void ItShouldBePossibleToPartitionByMultipleColumns()
        {
            var expr = Select.Column<Person>(p => p.Name)
                             .Column<Person>(p => Sql.Max(p.Age), Over.PartitionBy<Person>(p => p.Name).ThenBy<Person>(p => p.LivesAtHouseId))
                             .From<Person>();
            var result = expr.ToSqlExpression();

            Assert.Equal(TokenGeneration_SelectPartitionTests_Results.partitionByMultipleColumnsExpression, result);
        }

        [Fact]
        public void ItShouldBePossibleToOrderByAColumn()
        {
            var expr = Select.Column<Person>(p => p.Name)
                             .Column<Person>(p => Sql.Sum(p.Age), Over.OrderBy<Person>(p => p.Age))
                             .From<Person>();
            var result = expr.ToSqlExpression();
            
            Assert.Equal(TokenGeneration_SelectPartitionTests_Results.orderByColumnExpression, result);
        }

        [Fact]
        public void ItShouldBePossibleToOrderByMultipleColumns()
        {
            var expr = Select.Column<Person>(p => p.Name)
                             .Column<Person>(p => Sql.Sum(p.Age), Over.OrderBy<Person>(p => p.Age, Direction.Ascending, NullsOrdering.NullsLast).ThenBy<Person>(p => p.Name, Direction.Descending))
                             .From<Person>();
            var result = expr.ToSqlExpression();
            
            Assert.Equal(TokenGeneration_SelectPartitionTests_Results.orderByMultipleColumnsExpression, result);
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
            
            Assert.Equal(TokenGeneration_SelectPartitionTests_Results.orderAndPartitionByColumnExpression, result);
        }
    }
}
