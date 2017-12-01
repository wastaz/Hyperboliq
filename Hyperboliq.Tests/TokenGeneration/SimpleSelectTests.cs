using NUnit.Framework;
using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;

namespace Hyperboliq.Tests
{
    [TestFixture]
    public class TokenGeneration_SimpleSelectTests
    {
        [Test]
        public void ItShouldBeAbleToSelectDistinctFromATable()
        {
            var expr = Select.Distinct.Star<Person>()
                             .From<Person>();
            var result = expr.ToSqlExpression();

            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleSelectTests_Results.selectDistinctExpression));
        }

        [Test]
        public void ItShouldBePossibleToSelectAConstant()
        {
            var expr = Select.Column<Person>(p => new { favoriteNumber = 42, name = p.Name })
                             .From<Person>();
            var result = expr.ToSqlExpression();

            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleSelectTests_Results.selectConstantExpression));
        }

        [Test]
        public void ItShouldBeAbleToSelectSingleColumnsFromATable()
        {
            var expr = Select.Column<Person>(p => new { p.Name, p.Age })
                             .From<Person>();
            var result = expr.ToSqlExpression();

            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleSelectTests_Results.selectColumnsExpression));
        }

        [Test]
        public void ItShouldOrderTheColumnsInTheExceptedOrderWhenCallingColumnSeveralTimes()
        {
            var expr = Select.Column<Person>(p => p.Name).Column<Person>(p => p.Age)
                             .From<Person>();
            var result = expr.ToSqlExpression();

            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleSelectTests_Results.selectColumnsExpression));
        }

        [Test]
        public void ItShouldBeAbleToSelectDistinctSingleColumnsFromATable()
        {
            var expr = Select.Distinct.Column<Person>(p => p.Age)
                             .From<Person>();
            var result = expr.ToSqlExpression();

            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleSelectTests_Results.selectDistinctSingleColumnsExpression));
        }

        [Test]
        public void ItShouldBePossibleToSelectTheNumberOfRowsFromATable()
        {
            var expr = Select.Column<Person>(p => Sql.Count()).From<Person>();
            var result = expr.ToSqlExpression();

            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleSelectTests_Results.selectCountExpression));
        }

        [Test]
        public void ItShouldBePossibleToSelectTheNumberOfRowsFromATableAndNameTheColumn()
        {
            var expr = Select.Column<Person>(p => new { numberOfPersons = Sql.Count() }).From<Person>();
            var result = expr.ToSqlExpression();

            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleSelectTests_Results.selectNamedCountExpression));
        }
    }
}
