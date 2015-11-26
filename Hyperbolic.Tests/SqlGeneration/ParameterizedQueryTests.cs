using NUnit.Framework;
using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;
using BinaryOperation = Hyperboliq.Domain.AST.BinaryOperation;
using S = Hyperboliq.Tests.SqlStreamExtensions;

namespace Hyperboliq.Tests.SqlGeneration
{
    [TestFixture]
    public class SqlGeneration_ParameterizedQueryTests
    {
        [Test]
        public void ItShouldBeAbleToParameterizeAQuery()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Star<Person>()),
                    S.From<Person>(),
                    S.Where(S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Param("age"))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.That(result, Is.EqualTo("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age > @age"));
        }

        [Test]
        public void ItShouldBePossibleToUseTheParameterInMoreComplexExpressions()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Star<Person>()),
                    S.From<Person>(),
                    S.Where(                    
                        S.BinExp(
                            S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Param("age")),
                            BinaryOperation.And,
                            S.BinExp(S.Param("age"), BinaryOperation.LessThan, S.Const(90))
                            ),
                        S.Or(S.BinExp(S.Col<Person>("Age"), BinaryOperation.LessThan, S.Param("age")))
                    ));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.That(result,
                Is.EqualTo(
                    @"SELECT PersonRef.* " +
                    "FROM Person PersonRef " +
                    "WHERE PersonRef.Age > @age "+
                    "AND @age < 90 " +
                    "OR PersonRef.Age < @age"));
        }
    }
}
