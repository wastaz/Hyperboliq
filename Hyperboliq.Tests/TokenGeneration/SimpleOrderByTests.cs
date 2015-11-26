using NUnit.Framework;
using Hyperboliq.Tests.TokenGeneration;
using Direction = Hyperboliq.Domain.AST.Direction;

namespace Hyperboliq.Tests
{
    [TestFixture]
    public class TokenGeneration_SimpleOrderByTests
    {
        [Test]
        public void ItShouldBePossibleToOrderAscendingByAColumn()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .OrderBy<Person>(p => p.Age, Direction.Ascending);
            var result = expr.ToSqlExpression();
            var expected = TokenGeneration_SimpleOrderByTests_Results.GetOrderByDirectionExpression(Direction.Ascending);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBePossibleToOrderDescendingByAColumn()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .OrderBy<Person>(p => p.Age, Direction.Descending);
            var result = expr.ToSqlExpression();
            var expected = TokenGeneration_SimpleOrderByTests_Results.GetOrderByDirectionExpression(Direction.Descending);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBePossibleToOrderBySeveralColumns()
        {
            var expr = Select.Star<Person>().Star<Car>()
                             .From<Person>()
                             .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId)
                             .OrderBy<Person>(p => p.Age, Direction.Ascending)
                             .ThenBy<Car>(c => c.Brand, Direction.Descending);
            var result = expr.ToSqlExpression();
            Assert.That(result, Is.EqualTo(TokenGeneration_SimpleOrderByTests_Results.orderByMultipleColumnsExpression));
        }
    }
}
