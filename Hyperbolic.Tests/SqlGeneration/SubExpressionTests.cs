using Xunit;
using Hyperboliq.Tests.Model;
using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.Stream.BinaryOperation;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "Subexpressions")]
    public class SqlGeneration_SubExpressionTests
    {
        [Fact]
        public void ItShouldBePossibleToSqlifyACompareAgainstASubExpressionInAWhereExpression()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Star<Person>()),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(
                            S.Col<Person>("Age"),
                            BinaryOperation.GreaterThan,
                            S.SubExp(
                                S.Select(S.Col<Car>("Age")),
                                S.From<Car>(),
                                S.Where(S.BinExp(S.Col<Car>("Id"), BinaryOperation.Equal, S.Const(42)))))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age > (SELECT CarRef.Age FROM Car CarRef WHERE CarRef.Id = 42)", result);
        }

        [Fact]
        public void ItShouldBePossibleToSqlifyAQueryWithAnInSubExpression()
        {

            var stream =
                S.SelectNode(
                    S.Select(S.Star<Person>()),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(
                            S.Col<Person>("Id"),
                            BinaryOperation.In,
                            S.SubExp(
                                S.Select(S.Col<Car>("DriverId")),
                                S.From<Car>()))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Id IN (SELECT CarRef.DriverId FROM Car CarRef)", result);
        }
    }
}
