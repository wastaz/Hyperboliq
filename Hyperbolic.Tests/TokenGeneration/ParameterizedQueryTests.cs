using Xunit;
using Hyperboliq.Tests.Model;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.Stream.BinaryOperation;

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
            var expected =
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>(),
                    S.Where(S.BinExp(S.Col<Person>("Age"), BinaryOperation.LessThan, S.Param("age")))
                    );

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBeAbleToParameterizeAQuery()
        {
            var ageParam = new ExpressionParameter<int>("age");
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .Where<Person>(p => p.Age > ageParam);
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>(),
                    S.Where(S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Param("age"))));

            Assert.Equal(expected, result);
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

            var expected =
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(
                            S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Param("age")),
                            BinaryOperation.And,
                            S.BinExp(S.Param("age"), BinaryOperation.LessThan, S.Const(90))
                            ),
                        S.Or(S.BinExp(S.Col<Person>("Age"), BinaryOperation.LessThan, S.Param("age")))));
            Assert.Equal(expected, result);
        }
    }
}
