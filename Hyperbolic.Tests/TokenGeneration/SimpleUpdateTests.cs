using Xunit;
using Hyperboliq.Tests.Model;
using Hyperboliq.Domain;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.AST.BinaryOperation;
using AggregateType = Hyperboliq.Domain.AST.AggregateType;

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
                S.UpdateNode(
                    S.UpdHead<Person>(
                        S.Ust<Person>("Name", "'Kalle'")));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToSetMultipleValues()
        {
            var expr = Update<Person>.Set(p => p.Name, "Kalle")
                                     .Set(p => p.Age, 42);
            var result = expr.ToSqlExpression();

            var expected =
                S.UpdateNode(
                    S.UpdHead<Person>(
                        S.Ust<Person>("Age", 42),
                        S.Ust<Person>("Name", "'Kalle'")));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToSetMultipleValueInASingleStatement()
        {
            var expr = Update<Person>.Set(p => new { p.Name, p.Age }, 
                                          new { Name = "Kalle", Age = 42});
            var result = expr.ToSqlExpression();

            var expected =
                S.UpdateNode(
                    S.UpdHead<Person>(
                        S.Ust<Person>("Name", "'Kalle'"),
                        S.Ust<Person>("Age", 42)));

            Assert.Equal(expected, result);
        }


        [Fact]
        public void ItShouldBePossibleToUpdateInPlace()
        {
            var expr = Update<Person>.Set(p => p.Age, p => p.Age + 1);
            var result = expr.ToSqlExpression();

            var expected =
                S.UpdateNode(
                    S.UpdHead<Person>(
                        S.Ust<Person>("Age", S.BinExp(S.Col<Person>("Age"), BinaryOperation.Add, S.Const(1)))));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToUpdateMultipleInPlace()
        {
            var expr = Update<Person>.Set(p => new { p.Name, p.Age },
                                          p => new { Name = "Kalle" + p.Name, Age = p.Age - 2 });
            var result = expr.ToSqlExpression();

            var expected =
                S.UpdateNode(
                    S.UpdHead<Person>(
                        S.Ust<Person>("Name", S.BinExp(S.Const("'Kalle'"), BinaryOperation.Add, S.Col<Person>("Name"))),
                        S.Ust<Person>("Age", S.BinExp(S.Col<Person>("Age"), BinaryOperation.Subtract, S.Const(2)))
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
                S.UpdateNode(
                    S.UpdHead<Person>(
                        S.Ust<Person>(
                            "Age", 
                            S.SubExp(
                                 S.Select(S.Aggregate(AggregateType.Max, S.Col<Car>("Age"))),
                                S.From<Car>()))));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToPerformAConditionalUpdate()
        {
            var expr = Update<Person>.Set(p => p.Age, 42)
                                     .Where(p => p.Name == "Kalle");
            var result = expr.ToSqlExpression();

            var expected =
                S.UpdateNode(
                    S.UpdHead<Person>(S.Ust<Person>("Age", 42)),
                    S.Where(S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Kalle'"))));
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
                S.UpdateNode(
                    S.UpdHead<Person>(S.Ust<Person>("Age", 42)),
                    S.Where(
                        S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Kalle'")),
                        S.And(S.BinExp(S.Col<Person>("Age"), BinaryOperation.LessThan, S.Const(18))),
                        S.Or(S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Pelle'")))
                        ));
            Assert.Equal(expected, result);
        }
    }
}
