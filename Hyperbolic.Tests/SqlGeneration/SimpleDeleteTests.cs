using NUnit.Framework;
using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;
using BinaryOperation = Hyperboliq.Domain.AST.BinaryOperation;
using S = Hyperboliq.Tests.SqlStreamExtensions;

namespace Hyperboliq.Tests.SqlGeneration
{
    [TestFixture]
    public class SqlGeneration_SimpleDeleteTests
    {
        [Test]
        public void ItShouldBePossibleToGenerateADeleteAllRowsFromTableStatement()
        {
            var stream =
                S.DeleteNode(S.From<Person>());
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.That(result, Is.EqualTo(@"DELETE FROM Person PersonRef"));
        }

        [Test]
        public void ItShouldBePossibleToPutAWhereExpressionOnADeleteStatement()
        {
            var stream =
                S.DeleteNode(
                    S.From<Person>(),
                    S.Where(S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(42))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.That(result, Is.EqualTo(@"DELETE FROM Person PersonRef WHERE PersonRef.Age > 42"));
        }

        [Test]
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

            Assert.That(result,
                Is.EqualTo(
                    "DELETE FROM Person PersonRef " +
                    "WHERE PersonRef.Age > 42 " +
                    "AND PersonRef.Name = 'Kalle' " +
                    "OR PersonRef.Name = 'Henrik'"));
        }

        [Test]
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

            Assert.That(
                result,
                Is.EqualTo(
                    "DELETE FROM Car CarRef " +
                    "WHERE CarRef.DriverId IN (" +
                        "SELECT PersonRef.Id " +
                        "FROM Person PersonRef " +
                        "WHERE PersonRef.Age < 18" +
                    ")"));
        }
    }
}
