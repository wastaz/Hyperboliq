using Xunit;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.SqlGenerator;
using FluentAssertions;

namespace Hyperboliq.Tests.SqlGeneration
{
    /*
    [Trait("SqlGeneration", "Delete")]
    public class SqlGeneration_SimpleDeleteTests
    {
        [Fact]
        public void ItShouldBePossibleToGenerateADeleteAllRowsFromTableStatement()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Delete),
                    Kw(KeywordNode.From),
                    Tbl<Person>());
            var result = SqlifySeq(AnsiSql.Dialect, stream);

            result.Should().Be(@"DELETE FROM Person PersonRef");
        }

        [Fact]
        public void ItShouldBePossibleToPutAWhereExpressionOnADeleteStatement()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Delete),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.Where),
                    BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42))
                    );
            var result = SqlifySeq(AnsiSql.Dialect, stream);

            result.Should().Be(@"DELETE FROM Person PersonRef WHERE PersonRef.Age > 42");
        }

        [Fact]
        public void ItShouldBePossibleToUseAndAndOrOnAWhereStatementOnADeleteStatement()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Delete),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.Where),
                    BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42)),
                    Kw(KeywordNode.And),
                    BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Kalle'")),
                    Kw(KeywordNode.Or),
                    BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Henrik'"))
                    );
            var result = SqlifySeq(AnsiSql.Dialect, stream);

            result.Should().Be(
                "DELETE FROM Person PersonRef " +
                "WHERE PersonRef.Age > 42 " +
                "AND PersonRef.Name = 'Kalle' " +
                "OR PersonRef.Name = 'Henrik'");
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

            result.Should().Be(
                "DELETE FROM Car CarRef " +
                "WHERE CarRef.DriverId IN (" +
                    "SELECT PersonRef.Id " +
                    "FROM Person PersonRef " +
                    "WHERE PersonRef.Age < 18" +
                ")");
        }
    }*/
}
