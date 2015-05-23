using Xunit;
using Hyperboliq.Tests.Model;
using Hyperboliq.Dialects;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.SqlGen;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "Delete")]
    public class SqlGeneration_SimpleDeleteTests
    {
        [Fact]
        public void ItShouldBePossibleToGenerateADeleteAllRowsFromTableStatement()
        {
            var stream =
                DeleteNode(From<Person>());
            var result = SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal(@"DELETE FROM Person PersonRef", result);
        }

        [Fact]
        public void ItShouldBePossibleToPutAWhereExpressionOnADeleteStatement()
        {
            var stream =
                DeleteNode(
                    From<Person>(),
                    Where(BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42))));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal(@"DELETE FROM Person PersonRef WHERE PersonRef.Age > 42", result);
        }

        [Fact]
        public void ItShouldBePossibleToUseAndAndOrOnAWhereStatementOnADeleteStatement()
        {
            var stream =
                DeleteNode(
                    From<Person>(),
                    Where(
                        BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42)),
                        And(BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Kalle'"))),
                        Or(BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Henrik'")))));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);

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
                DeleteNode(
                    From<Car>(),
                    Where(
                        BinExp(
                            Col<Car>("DriverId"),
                            BinaryOperation.In,
                            SubExp(
                                Select(Col<Person>("Id")),
                                From<Person>(),
                                Where(BinExp(Col<Person>("Age"), BinaryOperation.LessThan, Const(18)))))));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);

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
