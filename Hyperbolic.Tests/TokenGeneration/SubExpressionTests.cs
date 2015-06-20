using Xunit;
using Hyperboliq.Tests.Model;
using Hyperboliq.Domain;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.Stream.BinaryOperation;

namespace Hyperboliq.Tests
{
    [Trait("TokenGeneration", "Subexpressions")]
    public class TokenGeneration_SubExpressionTests
    {
        [Fact]
        public void ItShouldBePossibleToCompareAgainstASubExpressionInAWhereExpression()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .Where<Person>(p => p.Age > Sql.SubExpr<int>(Select.Column<Car>(c => c.Age)
                                                                                .From<Car>()
                                                                                .Where<Car>(c => c.Id == 42)));
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(
                            S.Col<Person>("Age"),
                            BinaryOperation.GreaterThan,
                            S.SubExp(
                                S.Select(S.Col<Car>("Age")),
                                S.From<Car>(),
                                S.Where(S.BinExp(S.Col<Car>("Id"), BinaryOperation.Equal, S.Const(42)))))));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToDoAnInQueryWithASubExpression()
        {
            var expr = 
                Select.Star<Person>()
                      .From<Person>()
                      .Where<Person>(p => Sql.In(p.Id, Select.Column<Car>(c => c.DriverId).From<Car>()));
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(
                            S.Col<Person>("Id"),
                            BinaryOperation.In,
                            S.SubExp(
                                S.Select(S.Col<Car>("DriverId")),
                                S.From<Car>()))));

            Assert.Equal(expected, result);
        }
    }
}
