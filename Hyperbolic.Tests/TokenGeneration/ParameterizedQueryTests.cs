using NUnit.Framework;
using Hyperboliq.Tests.TokenGeneration;

namespace Hyperboliq.Tests
{
    [TestFixture]
    public class TokenGeneration_ParameterizedQueryTests
    {
        [Test]
        public void ItShouldBeAbleToUseAOneOffParameterInAQuery()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .Where<Person>(p => p.Age < new ExpressionParameter<int>("age"));
            var result = expr.ToSqlExpression();

            Assert.That(result, Is.EqualTo(TokenGeneration_ParametrizedQueryTests_Result.oneOffParameterExpression));
        }

        [Test]
        public void ItShouldBeAbleToParameterizeAQuery()
        {
            var ageParam = new ExpressionParameter<int>("age");
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .Where<Person>(p => p.Age > ageParam);
            var result = expr.ToSqlExpression();

            Assert.That(result, Is.EqualTo(TokenGeneration_ParametrizedQueryTests_Result.parameterizedExpression));
        }

        [Test]
        public void ItShouldBePossibleToUseTheParameterInMoreComplexExpressions()
        {
            var ageParam = new ExpressionParameter<int>("age");
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .Where<Person>(p => p.Age > ageParam && ageParam < 90)
                             .Or<Person>(p => p.Age < ageParam);
            var result = expr.ToSqlExpression();

            Assert.That(result, Is.EqualTo(TokenGeneration_ParametrizedQueryTests_Result.complexParameterizedExpression));
        }
    }
}
