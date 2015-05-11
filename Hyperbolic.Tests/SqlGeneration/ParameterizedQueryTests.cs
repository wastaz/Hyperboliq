using Xunit;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.SqlGen;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "Parameters")]
    public class SqlGeneration_ParameterizedQueryTests
    {
        [Fact]
        public void ItShouldBeAbleToParameterizeAQuery()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    Where(BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Param("age"))));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age > @age", result);
        }

        [Fact]
        public void ItShouldBePossibleToUseTheParameterInMoreComplexExpressions()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    Where(                    
                        BinExp(
                            BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Param("age")),
                            BinaryOperation.And,
                            BinExp(Param("age"), BinaryOperation.LessThan, Const(90))
                            ),
                        Or(BinExp(Col<Person>("Age"), BinaryOperation.LessThan, Param("age")))
                    ));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal(
                @"SELECT PersonRef.* " +
                "FROM Person PersonRef " +
                "WHERE PersonRef.Age > @age "+
                "AND @age < 90 " +
                "OR PersonRef.Age < @age", result);
        }
    }
}
