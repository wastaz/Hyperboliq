using Xunit;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Tests.Model;
using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using BinaryOperation = Hyperboliq.Domain.Stream.BinaryOperation;

namespace Hyperboliq.Tests.SqlGeneration
{
    
    [Trait("SqlGeneration", "OperatorPredecence")]
    public class SqlGeneration_OperatorPredecenceTests
    {
        [Fact]
        public void ItShouldAddParensWhenNecessaryToPreserveAndOrPredecence()
        {
            var stream = 
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(
                            S.BinExp(
                               S.BinExp(S.Col<Person>("Age"), BinaryOperation.LessThan, S.Const(10)),
                               BinaryOperation.Or,
                               S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Karl'"))
                           ),
                           BinaryOperation.And,
                           S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(42))
                       )));

            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.* FROM Person PersonRef WHERE (PersonRef.Age < 10 OR PersonRef.Name = 'Karl') AND PersonRef.Age > 42", result);
        }
    }
}
