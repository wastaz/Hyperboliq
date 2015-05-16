using Xunit;
using static Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.SqlGen;

namespace Hyperboliq.Tests.SqlGeneration
{
    
    [Trait("SqlGeneration", "OperatorPredecence")]
    public class SqlGeneration_OperatorPredecenceTests
    {
        [Fact]
        public void ItShouldAddParensWhenNecessaryToPreserveAndOrPredecence()
        {
            var stream = 
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    Where(
                        BinExp(
                            BinExp(
                               BinExp(Col<Person>("Age"), BinaryOperation.LessThan, Const(10)),
                               BinaryOperation.Or,
                               BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Karl'"))
                           ),
                           BinaryOperation.And,
                           BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42))
                       )));

            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.* FROM Person PersonRef WHERE (PersonRef.Age < 10 OR PersonRef.Name = 'Karl') AND PersonRef.Age > 42", result);
        }
    }
}
