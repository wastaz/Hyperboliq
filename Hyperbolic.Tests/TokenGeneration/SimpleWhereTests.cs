using Xunit;
using Hyperboliq.Tests.Model;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.Types.BinaryOperation;
using JoinType = Hyperboliq.Domain.Stream.JoinType;

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
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>(),
                    S.Where(S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(42))));

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
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(
                            S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(42)),
                            BinaryOperation.Or,
                            S.BinExp(
                                S.BinExp(S.Col<Person>("Age"), BinaryOperation.LessThan, S.Const(10)),
                                BinaryOperation.And,
                                S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Karl'"))
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
                S.SelectNode(
                     S.Select(S.Col<Person>("*")),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(S.Col<Person>("Age"), BinaryOperation.LessThan, S.Const(42)),
                        S.Or(S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Karl'"))),
                        S.And(S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(12)))
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
                S.SelectNode(
                    S.Select(S.Col<Car>("*"), S.Col<Person>("*")),
                    S.From<Person>(
                        S.Join<Person, Car>(JoinType.InnerJoin, S.BinExp(S.Col<Person>("Id"), BinaryOperation.Equal, S.Col<Car>("DriverId")))),
                    S.Where(
                        S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(42)),
                        S.And(S.BinExp(S.Col<Car>("Brand"), BinaryOperation.Equal, S.Const("'SAAB'"))))
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
                S.SelectNode(
                    S.Select(S.Col<Car>("*"), S.Col<Person>("*")),
                    S.From<Person>(
                        S.Join<Person, Car>(JoinType.InnerJoin, S.BinExp(S.Col<Person>("Id"), BinaryOperation.Equal, S.Col<Car>("DriverId")))),
                    S.Where(S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Col<Car>("DriverId"))));
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
                S.SelectNode(
                    S.Select(S.Col<Car>("*"), S.Col<Person>("*")),
                    S.From<Person>(
                        S.Join<Person, Car>(JoinType.InnerJoin, S.BinExp(S.Col<Person>("Id"), BinaryOperation.Equal, S.Col<Car>("DriverId")))),
                    S.Where(
                        S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(42)),
                        S.Or(S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Col<Car>("Brand"))),
                        S.And(S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Col<Car>("Age")))
                        ));

            Assert.Equal(expected, result);
        }
    }
}
