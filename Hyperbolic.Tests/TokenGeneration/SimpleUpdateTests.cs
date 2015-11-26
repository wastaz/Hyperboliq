using NUnit.Framework;
using Hyperboliq.Domain;

namespace Hyperboliq.Tests.TokenGeneration
{
    [TestFixture]
    public class SimpleUpdateTests
    {
        [Test]
        public void ItShouldBePossibleToPerformAGlobalUpdate()
        {
            var expr = Update<Person>.Set(p => p.Name, "Kalle");
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleUpdateTests_Results.globalUpdateExpression));
        }

        [Test]
        public void ItShouldBePossibleToSetMultipleValues()
        {
            var expr = Update<Person>.Set(p => p.Age, 42)
                                     .Set(p => p.Name, "Kalle");
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleUpdateTests_Results.multipleSetsExpression));
        }

        [Test]
        public void ItShouldBePossibleToSetMultipleValueInASingleStatement()
        {
            var expr = Update<Person>.Set(p => new { p.Name, p.Age }, 
                                          new { Name = "Kalle", Age = 42});
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleUpdateTests_Results.multipleSetsExpression));
        }


        [Test]
        public void ItShouldBePossibleToUpdateInPlace()
        {
            var expr = Update<Person>.Set(p => p.Age, p => p.Age + 1);
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleUpdateTests_Results.updateInPlaceExpression));
        }

        [Test]
        public void ItShouldBePossibleToUpdateMultipleInPlace()
        {
            var expr = Update<Person>.Set(p => new { p.Name, p.Age },
                                          p => new { Name = "Kalle" + p.Name, Age = p.Age - 2 });
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleUpdateTests_Results.updateMultipleColumnsInPlaceExpression));
        }

        [Test]
        public void ItShouldBePossibleToUpdateValuesToASubExpression()
        {
            var expr = Update<Person>.Set(p => p.Age,
                                          Select.Column<Car>(c => Sql.Max(c.Age)).From<Car>());
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleUpdateTests_Results.updateValuesToSubExpression));
        }

        [Test]
        public void ItShouldBePossibleToPerformAConditionalUpdate()
        {
            var expr = Update<Person>.Set(p => p.Age, 42)
                                     .Where(p => p.Name == "Kalle");
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleUpdateTests_Results.conditionalUpdateExpression));
        }

        [Test]
        public void ItShouldBePossibleToHaveMultipleConditionsOnTheUpdate()
        {
            var expr = Update<Person>.Set(p => p.Age, 42)
                                     .Where(p => p.Name == "Kalle")
                                     .Or(p => p.Name == "Pelle")
                                     .And(p => p.Age < 18);
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleUpdateTests_Results.multipleConditionsUpdateExpression));
        }
    }
}
