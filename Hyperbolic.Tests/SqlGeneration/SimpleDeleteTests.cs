using Xunit;
using Hyperboliq.Tests.Model;
using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using BinaryOperation = Hyperboliq.Domain.Stream.BinaryOperation;
using S = Hyperboliq.Tests.SqlStreamExtensions;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "Delete")]
    public class SqlGeneration_SimpleDeleteTests
    {
        [Fact]
        public void ItShouldBePossibleToGenerateADeleteAllRowsFromTableStatement()
        {
            var stream =
                S.DeleteNode(S.From<Person>());
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal(@"DELETE FROM Person PersonRef", result);
        }

        [Fact]
        public void ItShouldBePossibleToPutAWhereExpressionOnADeleteStatement()
        {
            var stream =
                S.DeleteNode(
                    S.From<Person>(),
                    S.Where(S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(42))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal(@"DELETE FROM Person PersonRef WHERE PersonRef.Age > 42", result);
        }

        [Fact]
        public void ItShouldBePossibleToUseAndAndOrOnAWhereStatementOnADeleteStatement()
        {
            var stream =
                S.DeleteNode(
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(42)),
                        S.And(S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Kalle'"))),
                        S.Or(S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Henrik'")))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal(
                "DELETE FROM Person PersonRef " +
                "WHERE PersonRef.Age > 42 " +
                "AND PersonRef.Name = 'Kalle' " +
                "OR PersonRef.Name = 'Henrik'",
                result);
        }

        [Fact]
        public void ItShouldBePossibleToDeleteWithASubQuery()
        {
            var stream =
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
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal(
                "DELETE FROM Car CarRef " +
                "WHERE CarRef.DriverId IN (" +
                    "SELECT PersonRef.Id " +
                    "FROM Person PersonRef " +
                    "WHERE PersonRef.Age < 18" +
                ")",
                result);
        }
    }
}
