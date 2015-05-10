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
    [Trait("SqlGeneration", "OperatorPredecence")]
    public class SqlGeneration_OperatorPredecenceTests
    {
        [Fact]
        public void ItShouldAddParensWhenNecessaryToPreserveAndOrPredecence()
        {
            var stream = StreamFrom(
               Kw(KeywordNode.Select),
               Col<Person>("*"),
               Kw(KeywordNode.From),
               Tbl<Person>(),
               Kw(KeywordNode.Where),
               BinExp(
                   BinExp(
                       BinExp(Col<Person>("Age"), BinaryOperation.LessThan, Const(10)),
                       BinaryOperation.Or,
                       BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Karl'"))
                   ),
                   BinaryOperation.And,
                   BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42))
               ));

            var result = SqlifySeq(AnsiSql.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.* FROM Person PersonRef WHERE (PersonRef.Age < 10 OR PersonRef.Name = 'Karl') AND PersonRef.Age > 42");
        }
    }*/
}
