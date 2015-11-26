using NUnit.Framework;
using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;

namespace Hyperboliq.Tests
{
    [TestFixture]
    public class TokenGeneration_SimpleGroupByTests
    {
        [Test]
        public void ItShouldBePossibleToGroupByASingleColumn()
        {
            var expr = Select.Column<Person>(p => new { p.Name, maxAge = Sql.Max(p.Age) })
                             .From<Person>()
                             .GroupBy<Person>(p => p.Name);
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleGroupByTests_Results.groupBySingleColumnExpression));
        }

        [Test]
        public void ItShouldBePossibleToGroupByMultipleColumns()
        {
            var expr = Select.Column<Person>(p => new { p.Name, p.LivesAtHouseId, minAge = Sql.Min(p.Age) })
                             .From<Person>()
                             .GroupBy<Person>(p => new { p.Name, p.LivesAtHouseId });
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleGroupByTests_Results.groupByMultipleColumnsExpression));
        }
        
        [Test]
        public void ItShouldBePossibleToGroupByColumnsFromMultipleTables()
        {
            var expr = Select.Column<Person>(p => new { p.Name, averageAge = Sql.Avg(p.Age) })
                             .Column<Car>(c => new { c.Brand, minAge = Sql.Min(c.Age) })
                             .From<Person>()
                             .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId)
                             .GroupBy<Person>(p => p.Name)
                             .ThenBy<Car>(c => c.Brand);
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleGroupByTests_Results.groupByColumnsFromMultipleTablesExpression));
        }

        [Test]
        public void ItShouldBePossibleToUseASingleHavingExpression()
        {
            var expr = Select.Column<Person>(p => new { p.Name, averageAge = Sql.Avg(p.Age) })
                             .From<Person>()
                             .GroupBy<Person>(p => p.Name)
                             .Having<Person>(p => Sql.Avg(p.Age) > 42);
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleGroupByTests_Results.groupByWithSingleHavingExpression));
        }

        [Test]
        public void ItShouldBePossibleToUseMultipleHavingExpressions()
        {
            var expr = Select.Column<Person>(p => new { p.Name, averageAge = Sql.Avg(p.Age) })
                             .Column<Car>(c => new { c.Brand, minAge = Sql.Min(c.Age) })
                             .From<Person>()
                             .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId)
                             .GroupBy<Person>(p => p.Name).ThenBy<Car>(c => c.Brand)
                             .Having<Person>(p => Sql.Avg(p.Age) > 42)
                             .And<Car>(c => Sql.Min(c.Age) > 2);
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleGroupByTests_Results.groupByWithMultipleHavingExpression));
        }
    }
}
