using Xunit;
using Hyperboliq.Tests.TokenGeneration;

namespace Hyperboliq.Tests
{
    [Trait("TokenGeneration", "Parameters")]
    public class TokenGeneration_ParameterizedQueryTests
    {
        [Fact]
        public void ItShouldBeAbleToUseAOneOffParameterInAQuery()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .Where<Person>(p => p.Age < new ExpressionParameter<int>("age"));
            var result = expr.ToSqlExpression();

            Assert.Equal(TokenGeneration_ParametrizedQueryTests_Result.oneOffParameterExpression, result);
        }

        [Fact]
        public void ItShouldBeAbleToParameterizeAQuery()
        {
            var ageParam = new ExpressionParameter<int>("age");
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .Where<Person>(p => p.Age > ageParam);
            var result = expr.ToSqlExpression();

            Assert.Equal(TokenGeneration_ParametrizedQueryTests_Result.parameterizedExpression, result);
        }

        [Fact]
        public void ItShouldBePossibleToUseTheParameterInMoreComplexExpressions()
        {
            var ageParam = new ExpressionParameter<int>("age");
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .Where<Person>(p => p.Age > ageParam && ageParam < 90)
                             .Or<Person>(p => p.Age < ageParam);
            var result = expr.ToSqlExpression();

            Assert.Equal(TokenGeneration_ParametrizedQueryTests_Result.complexParameterizedExpression, result);
        }
    }
}
