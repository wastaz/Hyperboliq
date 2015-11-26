using NUnit.Framework;
using Hyperboliq.Domain;
using Direction = Hyperboliq.Domain.AST.Direction;
using NullsOrdering = Hyperboliq.Domain.AST.NullsOrdering;

namespace Hyperboliq.Tests.TokenGeneration
{
    [TestFixture]
    public class TokenGeneration_SelectPartitionsTests
    {
        [Test]
        public void ItShouldBePossibleToUseAnEmptyOverClause()
        {
            var expr = Select.Column<Person>(p => p.Name)
                             .Column<Person>(p => Sql.Sum(p.Age), Over.Empty)
                             .From<Person>();
            var result = expr.ToSqlExpression();

            Assert.That(result, Is.EqualTo(TokenGeneration_SelectPartitionTests_Results.emptyOverClauseExpression));
        }

        [Test]
        public void ItShouldBePossibleToPartitionByAColumn()
        {
            var expr = Select.Column<Person>(p => p.Name)
                             .Column<Person>(p => Sql.Max(p.Age), Over.PartitionBy<Person>(p => p.Name))
                             .From<Person>();
            var result = expr.ToSqlExpression();
            
            Assert.That(result, Is.EqualTo(TokenGeneration_SelectPartitionTests_Results.partitionByColumnExpression));
        }

        [Test]
        public void ItShouldBePossibleToPartitionByMultipleColumns()
        {
            var expr = Select.Column<Person>(p => p.Name)
                             .Column<Person>(p => Sql.Max(p.Age), Over.PartitionBy<Person>(p => p.Name).ThenBy<Person>(p => p.LivesAtHouseId))
                             .From<Person>();
            var result = expr.ToSqlExpression();

            Assert.That(result, Is.EqualTo(TokenGeneration_SelectPartitionTests_Results.partitionByMultipleColumnsExpression));
        }

        [Test]
        public void ItShouldBePossibleToOrderByAColumn()
        {
            var expr = Select.Column<Person>(p => p.Name)
                             .Column<Person>(p => Sql.Sum(p.Age), Over.OrderBy<Person>(p => p.Age))
                             .From<Person>();
            var result = expr.ToSqlExpression();
            
            Assert.That(result, Is.EqualTo(TokenGeneration_SelectPartitionTests_Results.orderByColumnExpression));
        }

        [Test]
        public void ItShouldBePossibleToOrderByMultipleColumns()
        {
            var expr = Select.Column<Person>(p => p.Name)
                             .Column<Person>(p => Sql.Sum(p.Age), Over.OrderBy<Person>(p => p.Age, Direction.Ascending, NullsOrdering.NullsLast).ThenBy<Person>(p => p.Name, Direction.Descending))
                             .From<Person>();
            var result = expr.ToSqlExpression();
            
            Assert.That(result, Is.EqualTo(TokenGeneration_SelectPartitionTests_Results.orderByMultipleColumnsExpression));
        }

        [Test]
        public void ItShouldBePossibleToBothPartitionAndOrderByAColumn()
        {
            var expr = Select.Column<Person>(p => p.Name)
                             .Column<Person>(
                                p => Sql.Sum(p.Age), 
                                Over.PartitionBy<Person>(p => p.Name).OrderBy<Person>(p => p.Age))
                             .From<Person>();
            var result = expr.ToSqlExpression();
            
            Assert.That(result, Is.EqualTo(TokenGeneration_SelectPartitionTests_Results.orderAndPartitionByColumnExpression));
        }
    }
}
