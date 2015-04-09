using Xunit;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;

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
            var result = expr.ToSqlStream();
            var expected = StreamFrom(
                Kw(KeywordNode.Select),
                Col<Person>("*"),
                Kw(KeywordNode.From),
                Tbl<Person>(),
                Kw(KeywordNode.Where),
                BinExp(Col<Person>("Age"), BinaryOperation.LessThan, Param("age"))
                );

            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBeAbleToParameterizeAQuery()
        {
            var ageParam = new ExpressionParameter<int>("age");
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .Where<Person>(p => p.Age > ageParam);
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.Where),
                    BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Param("age"))
                    );

            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBePossibleToUseTheParameterInMoreComplexExpressions()
        {
            var ageParam = new ExpressionParameter<int>("age");
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .Where<Person>(p => p.Age > ageParam && ageParam < 90)
                             .Or<Person>(p => p.Age < ageParam);
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.Where),
                    BinExp(
                        BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Param("age")),
                        BinaryOperation.And,
                        BinExp(Param("age"), BinaryOperation.LessThan, Const(90))
                        ),
                    Kw(KeywordNode.Or),
                    BinExp(Col<Person>("Age"), BinaryOperation.LessThan, Param("age"))
                    );
            result.ShouldEqual(expected);
        }
    }
}
