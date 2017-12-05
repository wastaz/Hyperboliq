using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;
using NUnit.Framework;

namespace Hyperboliq.Tests.SqlServer
{
    [TestFixture]
    public class LimitOffsetTests
    {
        [Test]
        public void ItShouldBePossibleToUseBothLimitAndOffset()
        {
            var expr = TokenGeneration_LimitOffsetTests_Result.limitOffsetExpression;
            var result = SqlGen.SqlifyExpression(Dialects.SqlServer.Dialect, expr);
            var expected =
                "SELECT AnimalRef.* FROM [Animal] [AnimalRef] ORDER BY [AnimalRef].[Name] ASC OFFSET 15 ROWS FETCH NEXT 10 ROWS ONLY";
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBePossibleToUseOnlyLimit()
        {
            var expr = TokenGeneration_LimitOffsetTests_Result.limitExpression;
            var result = SqlGen.SqlifyExpression(Dialects.SqlServer.Dialect, expr);
            var expected =
                "SELECT AnimalRef.* FROM [Animal] [AnimalRef] ORDER BY [AnimalRef].[Name] ASC OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY";
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBePossibleToUseOnlyOffset()
        {
            var expr = TokenGeneration_LimitOffsetTests_Result.offsetExpression;
            var result = SqlGen.SqlifyExpression(Dialects.SqlServer.Dialect, expr);
            var expected = 
                "SELECT AnimalRef.* FROM [Animal] [AnimalRef] ORDER BY [AnimalRef].[Name] ASC OFFSET 15 ROWS";
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBePossibleToUseNeitherLimitNorOffset()
        {
            var expr = TokenGeneration_LimitOffsetTests_Result.noLimitOrOffsetExpression;
            var result = SqlGen.SqlifyExpression(Dialects.SqlServer.Dialect, expr);
            var expected = 
                "SELECT AnimalRef.* FROM [Animal] [AnimalRef] ORDER BY [AnimalRef].[Name] ASC";
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}