using Xunit;
using static Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Tests.Model;
using Hyperboliq.Dialects;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.SqlGen;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "Subexpressions")]
    public class SqlGeneration_SubExpressionTests
    {
        [Fact]
        public void ItShouldBePossibleToSqlifyACompareAgainstASubExpressionInAWhereExpression()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    Where(
                        BinExp(
                            Col<Person>("Age"),
                            BinaryOperation.GreaterThan,
                            SubExp(
                                Select(Col<Car>("Age")),
                                From<Car>(),
                                Where(BinExp(Col<Car>("Id"), BinaryOperation.Equal, Const(42)))))));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age > (SELECT CarRef.Age FROM Car CarRef WHERE CarRef.Id = 42)", result);
        }

        [Fact]
        public void ItShouldBePossibleToSqlifyAQueryWithAnInSubExpression()
        {

            var stream =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    Where(
                        BinExp(
                            Col<Person>("Id"),
                            BinaryOperation.In,
                            SubExp(
                                Select(Col<Car>("DriverId")),
                                From<Car>()))));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Id IN (SELECT CarRef.DriverId FROM Car CarRef)", result);
        }
    }
}
