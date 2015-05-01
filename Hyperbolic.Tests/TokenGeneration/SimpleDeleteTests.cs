using Xunit;
using Hyperboliq.FluentApi;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;

namespace Hyperboliq.Tests.TokenGeneration
{
    [Trait("TokenGeneration", "Delete")]
    public class TokenGeneration_SimpleDeleteTests
    {
        [Fact]
        public void ItShouldBePossibleToGenerateADeleteAllRowsFromTableStatement()
        {
            var expr = Delete.From<Person>();
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Kw(KeywordNode.Delete),
                    Kw(KeywordNode.From),
                    Tbl<Person>());
            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBePossibleToPutAWhereExpressionOnADeleteStatement()
        {
            var expr = Delete.From<Person>().Where<Person>(p => p.Age > 42);
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Kw(KeywordNode.Delete),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Where(BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42)))
                    );
            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBePossibleToUseAndAndOrOnAWhereStatementOnADeleteStatement()
        {
            var expr = Delete.From<Person>()
                             .Where<Person>(p => p.Age > 42)
                             .And<Person>(p => p.Name == "Kalle")
                             .Or<Person>(p => p.Name == "Henrik");
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Kw(KeywordNode.Delete),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Where(
                        BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42)),
                        Or(BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Henrik'"))),
                        And(BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Kalle'")))
                    ));
            result.ShouldEqual(expected);
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
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Kw(KeywordNode.Delete),
                    Kw(KeywordNode.From),
                    Tbl<Car>(),
                    Where(
                        BinExp(
                            Col<Car>("DriverId"),
                            BinaryOperation.In,
                            SubExp(StreamFrom(
                                Select(Col<Person>("Id")),
                                Kw(KeywordNode.From),
                                Tbl<Person>(),
                                Where(BinExp(Col<Person>("Age"), BinaryOperation.LessThan, Const(18))))))));
            result.ShouldEqual(expected);
        }
    }
}
