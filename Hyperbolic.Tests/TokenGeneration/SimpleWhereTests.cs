using Xunit;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;

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

            var expected =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    Where(BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42))));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldHandleAWhereConditionWithAndAndOrsInTheExpression()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .Where<Person>(p => p.Age > 42 || (p.Age < 10 && p.Name == "Karl"));
            var result = expr.ToSqlExpression();

            var expected =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    Where(
                        BinExp(
                            BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42)),
                            BinaryOperation.Or,
                            BinExp(
                                BinExp(Col<Person>("Age"), BinaryOperation.LessThan, Const(10)),
                                BinaryOperation.And,
                                BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Karl'"))
                            )
                    )));
            Assert.Equal(expected, result);
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

            var expected =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    Where(
                        BinExp(Col<Person>("Age"), BinaryOperation.LessThan, Const(42)),
                        Or(BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Karl'"))),
                        And(BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(12)))
                    ));

            Assert.Equal(expected, result);
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

            var expected =
                SelectNode(
                    Select(Col<Car>("*"), Col<Person>("*")),
                    From<Person>(
                        Join<Person, Car>(JoinType.InnerJoin, BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")))),
                    Where(
                        BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42)),
                        And(BinExp(Col<Car>("Brand"), BinaryOperation.Equal, Const("'SAAB'"))))
                    );
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBeAbleToReferenceSeveralTablesInAWhereCondition()
        {
            var expr = Select.Star<Person>().Star<Car>()
                             .From<Person>()
                             .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId)
                             .Where<Person, Car>((p, c) => p.Age > c.DriverId);
            var result = expr.ToSqlExpression();

            var expected =
                SelectNode(
                    Select(Col<Car>("*"), Col<Person>("*")),
                    From<Person>(
                        Join<Person, Car>(JoinType.InnerJoin, BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")))),
                    Where(BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Col<Car>("DriverId"))));
            Assert.Equal(expected, result);
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

            var expected =
                SelectNode(
                    Select(Col<Car>("*"), Col<Person>("*")),
                    From<Person>(
                        Join<Person, Car>(JoinType.InnerJoin, BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")))),
                    Where(
                        BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42)),
                        Or(BinExp(Col<Person>("Name"), BinaryOperation.Equal, Col<Car>("Brand"))),
                        And(BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Col<Car>("Age")))
                        ));

            Assert.Equal(expected, result);
        }
    }
}
