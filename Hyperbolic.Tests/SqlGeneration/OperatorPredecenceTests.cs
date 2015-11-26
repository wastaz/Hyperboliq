using NUnit.Framework;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;
using BinaryOperation = Hyperboliq.Domain.AST.BinaryOperation;

namespace Hyperboliq.Tests.SqlGeneration
{
    
    [TestFixture]
    public class SqlGeneration_OperatorPredecenceTests
    {
        [Test]
        public void ItShouldAddParensWhenNecessaryToPreserveAndOrPredecence()
        {
            var stream = 
                S.SelectNode(
                    S.Select(S.Star<Person>()),
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
            Assert.That(result, Is.EqualTo(@"SELECT PersonRef.* FROM Person PersonRef WHERE (PersonRef.Age < 10 OR PersonRef.Name = 'Karl') AND PersonRef.Age > 42"));
        }
    }
}
