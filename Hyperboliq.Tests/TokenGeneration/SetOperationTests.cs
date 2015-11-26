using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using NUnit.Framework;

namespace Hyperboliq.Tests.TokenGeneration
{
    [TestFixture]
    public class TokenGeneration_SetOperationTests
    {

        [Test]
        public void ItCanDoASimpleUnion()
        {
            var expr =
                SetOperations.Union(
                    Select.Star<Person>()
                          .From<Person>()
                          .Where<Person>(p => p.Age > 42),
                    Select.Star<Person>()
                          .From<Person>()
                          .Where<Person>(p => p.Name == "Kalle")
                    );
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SetOperationTests_Results.simpleUnionExpression));
        }

        [Test]
        public void ItCanDoASimpleUnionAll()
        {
            var expr =
                SetOperations.UnionAll(
                    Select.Star<Person>()
                          .From<Person>()
                          .Where<Person>(p => p.Age > 42),
                    Select.Star<Person>()
                          .From<Person>()
                          .Where<Person>(p => p.Name == "Kalle")
                    );
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SetOperationTests_Results.simpleUnionAllExpression));
        }

        [Test]
        public void ItCanDoASimpleIntersect()
        {
            var expr = SetOperations.Intersect(
                Select.Star<Person>()
                      .From<Person>(),
                Select.Star<Person>()
                      .From<Person>()
                      .Where<Person>(p => p.Age > 42));
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SetOperationTests_Results.simpleIntersectExpression));
        }

        [Test]
        public void ItCanDoASimpleMinus()
        {
            var expr = SetOperations.Minus(
                Select.Star<Person>()
                      .From<Person>(),
                Select.Star<Person>()
                      .From<Person>()
                      .Where<Person>(p => p.Age > 42));
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SetOperationTests_Results.simpleMinusExpression));
        }

        [Test]
        public void ItCanDoAUnionsWithCommonTableExpressions()
        {
            var identifier = Table<Person>.WithTableAlias("cte");
            var expr =
                With.Table(
                        identifier,
                        SetOperations.Union(
                            Select.Star<Person>().From<Person>().Where<Person>(p => p.Age > 42),
                            Select.Star<Person>().From<Person>().Where<Person>(p => p.Name == "Kalle")))
                    .Query(
                        SetOperations.Union(
                            Select.Star(identifier).From(identifier).Where<Person>(identifier, p => p.Age == 50),
                            Select.Star(identifier).From(identifier).Where<Person>(identifier, p => p.Name == "Kalle")));

            var result = expr.ToSqlExpression();
            var sql = SqlGen.SqlifyExpression(AnsiSql.Dialect, result);
            var sql2 = SqlGen.SqlifyExpression(AnsiSql.Dialect, TokenGeneration_SetOperationTests_Results.unionsInACommonTableExpression);
            Assert.That(result, Is.EqualTo(TokenGeneration_SetOperationTests_Results.unionsInACommonTableExpression));
        }
    }
}
