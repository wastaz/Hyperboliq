using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;
using NUnit.Framework;

namespace Hyperboliq.Tests.Sqllite
{
    [TestFixture]
    public class LimitOffsetTests
    {
        [Test]
        public void ItShouldBePossibleToUseBothLimitAndOffset()
        {
            var expr = TokenGeneration_LimitOffsetTests_Result.limitOffsetExpression;
            var result = SqlGen.SqlifyExpression(Dialects.SqlLite.Dialect, expr);
            var expected =
                @"SELECT AnimalRef.* FROM ""Animal"" ""AnimalRef"" ORDER BY ""AnimalRef"".""Name"" ASC LIMIT 10 OFFSET 15";
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBePossibleToUseOnlyLimit()
        {
            var expr = TokenGeneration_LimitOffsetTests_Result.limitExpression;
            var result = SqlGen.SqlifyExpression(Dialects.SqlLite.Dialect, expr);
            var expected =
                @"SELECT AnimalRef.* FROM ""Animal"" ""AnimalRef"" ORDER BY ""AnimalRef"".""Name"" ASC LIMIT 10";
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBePossibleToUseOnlyOffset()
        {
            var expr = TokenGeneration_LimitOffsetTests_Result.offsetExpression;
            var result = SqlGen.SqlifyExpression(Dialects.SqlLite.Dialect, expr);
            var expected = 
                @"SELECT AnimalRef.* FROM ""Animal"" ""AnimalRef"" ORDER BY ""AnimalRef"".""Name"" ASC OFFSET 15";
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBePossibleToUseNeitherLimitNorOffset()
        {
            var expr = TokenGeneration_LimitOffsetTests_Result.noLimitOrOffsetExpression;
            var result = SqlGen.SqlifyExpression(Dialects.SqlLite.Dialect, expr);
            var expected = 
                @"SELECT AnimalRef.* FROM ""Animal"" ""AnimalRef"" ORDER BY ""AnimalRef"".""Name"" ASC";
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}