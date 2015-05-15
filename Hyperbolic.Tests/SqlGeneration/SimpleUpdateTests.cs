using Xunit;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.SqlGen;

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
            Assert.Equal("UPDATE Person SET Name = 'Kalle'", result);
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
            Assert.Equal("UPDATE Person SET Name = 'Kalle', Age = 42", result);
        }


        [Fact]
        public void ItShouldBePossibleToUpdateInPlace()
        {
            var stream =
              UpdateNode(
                  UpdHead<Person>(
                      Ust<Person>("Age", BinExp(Col<Person>("Age"), BinaryOperation.Add, Const(1)))));
            
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal("UPDATE Person SET Age = Age + 1", result);
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
            Assert.Equal("UPDATE Person SET Age = Age - 2, Name = 'Kalle' + Name", result);
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
            Assert.Equal("UPDATE Person SET Age = (SELECT MAX(CarRef.Age) FROM Car CarRef)", result);
        }

        [Fact]
        public void ItShouldBePossibleToPerformAConditionalUpdate()
        {
            var stream =
                UpdateNode(
                    UpdHead<Person>(Ust<Person>("Age", 42)),
                    Where(BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Kalle'"))));

            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal("UPDATE Person SET Age = 42 WHERE Name = 'Kalle'", result);
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
            Assert.Equal("UPDATE Person SET Age = 42 WHERE Name = 'Kalle' OR Name = 'Pelle' AND Age < 18", result);
        }
    }
}
