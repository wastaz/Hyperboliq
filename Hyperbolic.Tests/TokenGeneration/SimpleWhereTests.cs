using Xunit;
using Hyperboliq.Tests.TokenGeneration;

namespace Hyperboliq.Tests
{
    [Trait("TokenGeneration", "Where")]
    public class TokenGeneration_SimpleWhereTests
    {
        [Fact]
        public void ItShouldHandleASimpleWhereCondition()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .Where<Person>(p => p.Age > 42);
            var result = expr.ToSqlExpression();
            Assert.Equal(TokenGeneration_SimpleWhereTests_Results.simpleWhereConditionExpression, result);
        }

        [Fact]
        public void ItShouldHandleAWhereConditionWithAndAndOrsInTheExpression()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .Where<Person>(p => p.Age > 42 || (p.Age < 10 && p.Name == "Karl"));
            var result = expr.ToSqlExpression();
            Assert.Equal(TokenGeneration_SimpleWhereTests_Results.conditionalWithAndAndOrsExpression, result);
        }

        [Fact]
        public void ItShouldHandleAWhereConditionWithAndAndOrsThatIsNotInTheExpression()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .Where<Person>(p => p.Age < 42)
                             .And<Person>(p => p.Age > 12)
                             .Or<Person>(p => p.Name == "Karl");
            var result = expr.ToSqlExpression();
            Assert.Equal(TokenGeneration_SimpleWhereTests_Results.conditionalWithAndAndOrsOutsideExpression, result);
        }


        [Fact]
        public void ItShouldBePossibleToMakeWhereConditionsOnJoinedTables()
        {
            var expr = Select.Star<Person>().Star<Car>()
                             .From<Person>()
                             .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId)
                             .Where<Person>(p => p.Age > 42)
                             .And<Car>(c => c.Brand == "SAAB");
            var result = expr.ToSqlExpression();
            Assert.Equal(TokenGeneration_SimpleWhereTests_Results.conditionsOnJoinedTablesExpression, result);
        }

        [Fact]
        public void ItShouldBeAbleToReferenceSeveralTablesInAWhereCondition()
        {
            var expr = Select.Star<Person>().Star<Car>()
                             .From<Person>()
                             .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId)
                             .Where<Person, Car>((p, c) => p.Age > c.DriverId);
            var result = expr.ToSqlExpression();
            Assert.Equal(TokenGeneration_SimpleWhereTests_Results.conditionalReferencingSeveralTablesExpression, result);
        }

        [Fact]
        public void ItShouldBePossibleToReferenceSeveralTablesInAWhereConditionWithAndAndOr()
        {
            var expr = Select.Star<Person>().Star<Car>()
                             .From<Person>()
                             .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId)
                             .Where<Person>(p => p.Age > 42)
                             .And<Person, Car>((p, c) => p.Age > c.Age)
                             .Or<Person, Car>((p, c) => p.Name == c.Brand);
            var result = expr.ToSqlExpression();
            Assert.Equal(TokenGeneration_SimpleWhereTests_Results.conditionalReferencingSeveralTablesWithAndAndOrsExpression, result);
        }
    }
}
