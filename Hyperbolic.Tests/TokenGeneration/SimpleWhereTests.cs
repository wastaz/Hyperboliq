using System;
using FluentAssertions;
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
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.Where),
                    BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42))
                    );

            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldHandleAWhereConditionWithAndAndOrsInTheExpression()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .Where<Person>(p => p.Age > 42 || (p.Age < 10 && p.Name == "Karl"));
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.Where),
                    BinExp(
                        BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42)),
                        BinaryOperation.Or,
                        BinExp(
                            BinExp(Col<Person>("Age"), BinaryOperation.LessThan, Const(10)),
                            BinaryOperation.And,
                            BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Karl'"))
                        )
                    ));
            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldHandleAWhereConditionWithAndAndOrsThatIsNotInTheExpression()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .Where<Person>(p => p.Age < 42)
                             .And<Person>(p => p.Age > 12)
                             .Or<Person>(p => p.Name == "Karl");
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.Where),
                    BinExp(Col<Person>("Age"), BinaryOperation.LessThan, Const(42)),
                    Kw(KeywordNode.And),
                    BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(12)),
                    Kw(KeywordNode.Or),
                    BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Karl'"))
                    );

            result.ShouldEqual(expected);
        }


        [Fact]
        public void ItShouldBePossibleToMakeWhereConditionsOnJoinedTables()
        {
            var expr = Select.Star<Person>().Star<Car>()
                             .From<Person>()
                             .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId)
                             .Where<Person>(p => p.Age > 42)
                             .And<Car>(c => c.Brand == "SAAB");
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Col<Car>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.NewJoin(JoinType.InnerJoin)),
                    Tbl<Car>(),
                    Kw(KeywordNode.On),
                    BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")),
                    Kw(KeywordNode.Where),
                    BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42)),
                    Kw(KeywordNode.And),
                    BinExp(Col<Car>("Brand"), BinaryOperation.Equal, Const("'SAAB'"))
                    );
            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBeAbleToReferenceSeveralTablesInAWhereCondition()
        {
            var expr = Select.Star<Person>().Star<Car>()
                             .From<Person>()
                             .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId)
                             .Where<Person, Car>((p, c) => p.Age > c.DriverId);
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Col<Car>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.NewJoin(JoinType.InnerJoin)),
                    Tbl<Car>(),
                    Kw(KeywordNode.On),
                    BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")),
                    Kw(KeywordNode.Where),
                    BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Col<Car>("DriverId"))
                    );
            result.ShouldEqual(expected);
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
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Col<Car>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.NewJoin(JoinType.InnerJoin)),
                    Tbl<Car>(),
                    Kw(KeywordNode.On),
                    BinExp(Col<Person>("Id"), BinaryOperation.Equal, Col<Car>("DriverId")),
                    Kw(KeywordNode.Where),
                    BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42)),
                    Kw(KeywordNode.And),
                    BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Col<Car>("Age")),
                    Kw(KeywordNode.Or),
                    BinExp(Col<Person>("Name"), BinaryOperation.Equal, Col<Car>("Brand"))
                    );

            result.ShouldEqual(expected);
        }
    }
}
