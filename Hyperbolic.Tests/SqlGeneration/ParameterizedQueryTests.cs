using Xunit;
using Hyperboliq.Tests.Model;
using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using BinaryOperation = Hyperboliq.Domain.Stream.BinaryOperation;
using S = Hyperboliq.Tests.SqlStreamExtensions;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "Parameters")]
    public class SqlGeneration_ParameterizedQueryTests
    {
        [Fact]
        public void ItShouldBeAbleToParameterizeAQuery()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>(),
                    S.Where(S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Param("age"))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age > @age", result);
        }

        [Fact]
        public void ItShouldBePossibleToUseTheParameterInMoreComplexExpressions()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>(),
                    S.Where(                    
                        S.BinExp(
                            S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Param("age")),
                            BinaryOperation.And,
                            S.BinExp(S.Param("age"), BinaryOperation.LessThan, S.Const(90))
                            ),
                        S.Or(S.BinExp(S.Col<Person>("Age"), BinaryOperation.LessThan, S.Param("age")))
                    ));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal(
                @"SELECT PersonRef.* " +
                "FROM Person PersonRef " +
                "WHERE PersonRef.Age > @age "+
                "AND @age < 90 " +
                "OR PersonRef.Age < @age", result);
        }
    }
}
