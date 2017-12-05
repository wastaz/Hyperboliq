using NUnit.Framework;

namespace Hyperboliq.Tests.TokenGeneration
{
    [TestFixture]
    public class LimitOffsetTests
    {
        [Test]
        public void ItShouldBePossibleToUseBothLimitAndOffset()
        {
            var expr = Select.Star<Animal>().From<Animal>().OrderBy<Animal>(a => a.Name).Limit(10).Offset(15);
            var result = expr.ToSqlExpression();
            var expected = TokenGeneration_LimitOffsetTests_Result.limitOffsetExpression;
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBePossibleToUseOnlyLimit()
        {
            var expr = Select.Star<Animal>().From<Animal>().OrderBy<Animal>(a => a.Name).Limit(10);
            var result = expr.ToSqlExpression();
            var expected = TokenGeneration_LimitOffsetTests_Result.limitExpression;
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBePossibleToUseOnlyOffset()
        {
            var expr = Select.Star<Animal>().From<Animal>().OrderBy<Animal>(a => a.Name).Offset(15);
            var result = expr.ToSqlExpression();
            var expected = TokenGeneration_LimitOffsetTests_Result.offsetExpression;
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBePossibleToUseNeitherLimitNorOffset()
        {
            var expr = Select.Star<Animal>().From<Animal>().OrderBy<Animal>(a => a.Name);
            var result = expr.ToSqlExpression();
            var expected = TokenGeneration_LimitOffsetTests_Result.noLimitOrOffsetExpression;
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}