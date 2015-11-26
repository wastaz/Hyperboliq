using Xunit;

namespace Hyperboliq.Tests.TokenGeneration
{
    [Trait("TokenGeneration", "SetOperations")]
    public class TokenGeneration_SetOperationTests
    {

        [Fact]
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
            Assert.Equal(TokenGeneration_SetOperationTests_Results.simpleUnionExpression, result);
        }

        [Fact]
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
            Assert.Equal(TokenGeneration_SetOperationTests_Results.simpleUnionAllExpression, result);
        }

        [Fact]
        public void ItCanDoASimpleIntersect()
        {
            var expr = SetOperations.Intersect(
                Select.Star<Person>()
                      .From<Person>(),
                Select.Star<Person>()
                      .From<Person>()
                      .Where<Person>(p => p.Age > 42));
            var result = expr.ToSqlExpression();
            Assert.Equal(TokenGeneration_SetOperationTests_Results.simpleIntersectExpression, result);
        }

        [Fact]
        public void ItCanDoASimpleMinus()
        {
            var expr = SetOperations.Minus(
                Select.Star<Person>()
                      .From<Person>(),
                Select.Star<Person>()
                      .From<Person>()
                      .Where<Person>(p => p.Age > 42));
            var result = expr.ToSqlExpression();
            Assert.Equal(TokenGeneration_SetOperationTests_Results.simpleMinusExpression, result);
        }

        [Fact]
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
            Assert.Equal(TokenGeneration_SetOperationTests_Results.unionsInACommonTableExpression, result);
        }
    }
}
