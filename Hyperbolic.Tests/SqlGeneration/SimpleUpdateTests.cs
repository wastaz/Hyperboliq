using Xunit;
using FluentAssertions;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.SqlGenerator;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "Update")]
    public class SimpleUpdateTests
    {
        [Fact]
        public void ItShouldBePossibleToPerformAGlobalUpdate()
        {
            var stream =
                UpdateNode(
                    UpdHead<Person>(
                        Ust<Person>("Name", "'Kalle'")));

            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            result.Should().Be("UPDATE Person SET Name = 'Kalle'");
        }

        [Fact]
        public void ItShouldBePossibleToSetMultipleValues()
        {
            var stream =
              UpdateNode(
                  UpdHead<Person>(
                      Ust<Person>("Name", "'Kalle'"),
                      Ust<Person>("Age", 42)));

            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            result.Should().Be("UPDATE Person SET Name = 'Kalle', Age = 42");
        }


        [Fact]
        public void ItShouldBePossibleToUpdateInPlace()
        {
            var stream =
              UpdateNode(
                  UpdHead<Person>(
                      Ust<Person>("Age", BinExp(Col<Person>("Age"), BinaryOperation.Add, Const(1)))));
            
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            result.Should().Be("UPDATE Person SET Age = Age + 1");
        }

        [Fact]
        public void ItShouldBePossibleToUpdateMultipleInPlace()
        {
            var stream =
               UpdateNode(
                   UpdHead<Person>(
                       Ust<Person>("Age", BinExp(Col<Person>("Age"), BinaryOperation.Subtract, Const(2))),
                       Ust<Person>("Name", BinExp(Const("'Kalle'"), BinaryOperation.Add, Col<Person>("Name")))));
            
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            result.Should().Be("UPDATE Person SET Age = Age - 2, Name = 'Kalle' + Name");
        }

        [Fact]
        public void ItShouldBePossibleToUpdateValuesToASubExpression()
        {
            var stream =
                UpdateNode(
                    UpdHead<Person>(
                        Ust<Person>(
                            "Age",
                            SubExp(
                                Select(Aggregate(AggregateType.Max, Col<Car>("Age"))),
                                From<Car>()))));
            
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            result.Should().Be("UPDATE Person SET Age = (SELECT MAX(CarRef.Age) FROM Car CarRef)");
        }

        [Fact]
        public void ItShouldBePossibleToPerformAConditionalUpdate()
        {
            var stream =
                UpdateNode(
                    UpdHead<Person>(Ust<Person>("Age", 42)),
                    Where(BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Kalle'"))));

            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            result.Should().Be("UPDATE Person SET Age = 42 WHERE Name = 'Kalle'");
        }

        [Fact]
        public void ItShouldBePossibleToHaveMultipleConditionsOnTheUpdate()
        {
            var stream =
                UpdateNode(
                    UpdHead<Person>(Ust<Person>("Age", 42)),
                    Where(
                        BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Kalle'")),
                        Or(BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Pelle'"))),
                        And(BinExp(Col<Person>("Age"), BinaryOperation.LessThan, Const(18)))));

            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            result.Should().Be("UPDATE Person SET Age = 42 WHERE Name = 'Kalle' OR Name = 'Pelle' AND Age < 18");
        }
    }
}
