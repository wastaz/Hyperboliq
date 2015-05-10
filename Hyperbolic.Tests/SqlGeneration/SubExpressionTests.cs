using Xunit;
using static Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.SqlGenerator;
using FluentAssertions;

namespace Hyperboliq.Tests.SqlGeneration
{
    /*
    [Trait("SqlGeneration", "Subexpressions")]
    public class SqlGeneration_SubExpressionTests
    {
        [Fact]
        public void ItShouldBePossibleToSqlifyACompareAgainstASubExpressionInAWhereExpression()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.Where),
                    BinExp(
                        Col<Person>("Age"),
                        BinaryOperation.GreaterThan,
                        SubExp(
                            Select(Col<Car>("Age")),
                            From<Car>(),
                            Where(BinExp(Col<Car>("Id"), BinaryOperation.Equal, Const(42))))));
            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age > (SELECT CarRef.Age FROM Car CarRef WHERE CarRef.Id = 42)");
        }

        [Fact]
        public void ItShouldBePossibleToSqlifyAQueryWithAnInSubExpression()
        {

            var stream =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.Where),
                    BinExp(
                        Col<Person>("Id"),
                        BinaryOperation.In,
                        SubExp(
                            Select(Col<Car>("DriverId")),
                            From<Car>())));
            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Id IN (SELECT CarRef.DriverId FROM Car CarRef)");
        }
    }
    */
}
