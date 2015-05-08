using Xunit;
using Hyperboliq.FluentApi;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Tests.SqlStreamExtensions;

namespace Hyperboliq.Tests.TokenGeneration
{
    [Trait("TokenGeneration", "Update")]
    public class SimpleUpdateTests
    {
        [Fact]
        public void ItShouldBePossibleToPerformAGlobalUpdate()
        {
            var expr = Update<Person>.Set(p => p.Name, "Kalle");
            var result = expr.ToSqlExpression();

            var expected =
                UpdateNode(
                    UpdHead<Person>(
                        Ust<Person>("Name", "'Kalle'")));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToSetMultipleValues()
        {
            var expr = Update<Person>.Set(p => p.Name, "Kalle")
                                     .Set(p => p.Age, 42);
            var result = expr.ToSqlExpression();

            var expected =
                UpdateNode(
                    UpdHead<Person>(
                        Ust<Person>("Age", 42),
                        Ust<Person>("Name", "'Kalle'")));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToSetMultipleValueInASingleStatement()
        {
            var expr = Update<Person>.Set(p => new { p.Name, p.Age }, 
                                          new { Name = "Kalle", Age = 42});
            var result = expr.ToSqlExpression();

            var expected =
                UpdateNode(
                    UpdHead<Person>(
                        Ust<Person>("Name", "'Kalle'"),
                        Ust<Person>("Age", 42)));

            Assert.Equal(expected, result);
        }


        [Fact]
        public void ItShouldBePossibleToUpdateInPlace()
        {
            var expr = Update<Person>.Set(p => p.Age, p => p.Age + 1);
            var result = expr.ToSqlExpression();

            var expected =
                UpdateNode(
                    UpdHead<Person>(
                        Ust<Person>("Age", BinExp(Col<Person>("Age"), BinaryOperation.Add, Const(1)))));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToUpdateMultipleInPlace()
        {
            var expr = Update<Person>.Set(p => new { p.Name, p.Age },
                                          p => new { Name = "Kalle" + p.Name, Age = p.Age - 2 });
            var result = expr.ToSqlExpression();

            var expected =
                UpdateNode(
                    UpdHead<Person>(
                        Ust<Person>("Name", BinExp(Const("'Kalle'"), BinaryOperation.Add, Col<Person>("Name"))),
                        Ust<Person>("Age", BinExp(Col<Person>("Age"), BinaryOperation.Subtract, Const(2)))
                    ));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToUpdateValuesToASubExpression()
        {
            var expr = Update<Person>.Set(p => p.Age,
                                          Select.Column<Car>(c => Sql.Max(c.Age)).From<Car>());
            var result = expr.ToSqlExpression();

            var expected =
                UpdateNode(
                    UpdHead<Person>(
                        Ust<Person>(
                            "Age", 
                            SubExp(
                                Select(Aggregate(AggregateType.Max, Col<Car>("Age"))),
                                From<Car>()))));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToPerformAConditionalUpdate()
        {
            var expr = Update<Person>.Set(p => p.Age, 42)
                                     .Where(p => p.Name == "Kalle");
            var result = expr.ToSqlExpression();

            var expected =
                UpdateNode(
                    UpdHead<Person>(Ust<Person>("Age", 42)),
                    Where(BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Kalle'"))));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToHaveMultipleConditionsOnTheUpdate()
        {
            var expr = Update<Person>.Set(p => p.Age, 42)
                                     .Where(p => p.Name == "Kalle")
                                     .Or(p => p.Name == "Pelle")
                                     .And(p => p.Age < 18);
            var result = expr.ToSqlExpression();

            var expected =
                UpdateNode(
                    UpdHead<Person>(Ust<Person>("Age", 42)),
                    Where(
                        BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Kalle'")),
                        And(BinExp(Col<Person>("Age"), BinaryOperation.LessThan, Const(18))),
                        Or(BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Pelle'")))
                        ));
            Assert.Equal(expected, result);
        }
    }
}
