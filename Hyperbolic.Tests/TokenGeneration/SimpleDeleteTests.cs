using Xunit;
using Hyperboliq.Tests.Model;
using Hyperboliq.Domain;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.AST.BinaryOperation;

namespace Hyperboliq.Tests.TokenGeneration
{
    [Trait("TokenGeneration", "Delete")]
    public class TokenGeneration_SimpleDeleteTests
    {
        [Fact]
        public void ItShouldBePossibleToGenerateADeleteAllRowsFromTableStatement()
        {
            var expr = Delete.From<Person>();
            var result = expr.ToSqlExpression();

            var expected = S.DeleteNode(S.From<Person>());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToPutAWhereExpressionOnADeleteStatement()
        {
            var expr = Delete.From<Person>().Where<Person>(p => p.Age > 42);
            var result = expr.ToSqlExpression();

            var expected =
                S.DeleteNode(
                    S.From<Person>(),
                    S.Where(S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(42))));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToUseAndAndOrOnAWhereStatementOnADeleteStatement()
        {
            var expr = Delete.From<Person>()
                             .Where<Person>(p => p.Age > 42)
                             .And<Person>(p => p.Name == "Kalle")
                             .Or<Person>(p => p.Name == "Henrik");
            var result = expr.ToSqlExpression();

            var expected =
                S.DeleteNode(
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(42)),
                        S.Or(S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Henrik'"))),
                        S.And(S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Kalle'")))));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToDeleteWithASubQuery()
        {
            var expr = Delete.From<Car>()
                             .Where<Car>(c =>
                                Sql.In(
                                    c.DriverId,
                                    Select.Column<Person>(p => p.Id)
                                          .From<Person>()
                                          .Where<Person>(p => p.Age < 18)));
            var result = expr.ToSqlExpression();

            var expected =
                S.DeleteNode(
                    S.From<Car>(),
                    S.Where(
                        S.BinExp(
                            S.Col<Car>("DriverId"),
                            BinaryOperation.In,
                            S.SubExp(
                                S.Select(S.Col<Person>("Id")),
                                S.From<Person>(),
                                S.Where(S.BinExp(S.Col<Person>("Age"), BinaryOperation.LessThan, S.Const(18)))))));

            Assert.Equal(expected, result);
        }
    }
}
