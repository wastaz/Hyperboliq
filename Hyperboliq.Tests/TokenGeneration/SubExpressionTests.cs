using NUnit.Framework;
using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;

namespace Hyperboliq.Tests
{
    [TestFixture]
    public class TokenGeneration_SubExpressionTests
    {
        [Test]
        public void ItShouldBePossibleToCompareAgainstASubExpressionInAWhereExpression()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .Where<Person>(p => p.Age > Sql.SubExpr<int>(Select.Column<Car>(c => c.Age)
                                                                                .From<Car>()
                                                                                .Where<Car>(c => c.Id == 42)));
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SubExpression_Results.compareAgainstSubExprInWhereExpression));
        }

        [Test]
        public void ItShouldBePossibleToDoAnInQueryWithASubExpression()
        {
            var expr = 
                Select.Star<Person>()
                      .From<Person>()
                      .Where<Person>(p => Sql.In(p.Id, Select.Column<Car>(c => c.DriverId).From<Car>()));
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SubExpression_Results.subExprInInExpression));
        }
    }
}
