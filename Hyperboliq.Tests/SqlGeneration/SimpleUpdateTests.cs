using NUnit.Framework;
using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using AggregateType = Hyperboliq.Domain.AST.AggregateType;
using BinaryOperation = Hyperboliq.Domain.AST.BinaryOperation;

namespace Hyperboliq.Tests.SqlGeneration
{
    [TestFixture]
    public class SimpleUpdateTests
    {
        [Test]
        public void ItShouldBePossibleToPerformAGlobalUpdate()
        {
            var stream =
                S.UpdateNode(
                    S.UpdHead<Person>(
                        S.Ust<Person>("Name", "'Kalle'")));

            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.That(result, Is.EqualTo("UPDATE Person SET Name = 'Kalle'"));
        }

        [Test]
        public void ItShouldBePossibleToSetMultipleValues()
        {
            var stream =
              S.UpdateNode(
                  S.UpdHead<Person>(
                      S.Ust<Person>("Name", "'Kalle'"),
                      S.Ust<Person>("Age", 42)));

            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.That(result, Is.EqualTo("UPDATE Person SET Name = 'Kalle', Age = 42"));
        }


        [Test]
        public void ItShouldBePossibleToUpdateInPlace()
        {
            var stream =
              S.UpdateNode(
                  S.UpdHead<Person>(
                      S.Ust<Person>("Age", S.BinExp(S.Col<Person>("Age"), BinaryOperation.Add, S.Const(1)))));

            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.That(result, Is.EqualTo("UPDATE Person SET Age = Age + 1"));
        }

        [Test]
        public void ItShouldBePossibleToUpdateMultipleInPlace()
        {
            var stream =
               S.UpdateNode(
                   S.UpdHead<Person>(
                       S.Ust<Person>("Age", S.BinExp(S.Col<Person>("Age"), BinaryOperation.Subtract, S.Const(2))),
                       S.Ust<Person>("Name", S.BinExp(S.Const("'Kalle'"), BinaryOperation.Add, S.Col<Person>("Name")))));

            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.That(result, Is.EqualTo("UPDATE Person SET Age = Age - 2, Name = 'Kalle' + Name"));
        }

        [Test]
        public void ItShouldBePossibleToUpdateValuesToASubExpression()
        {
            var stream =
                S.UpdateNode(
                    S.UpdHead<Person>(
                        S.Ust<Person>(
                            "Age",
                            S.SubExp(
                                 S.Select(S.Aggregate(AggregateType.Max, S.Col<Car>("Age"))),
                                S.From<Car>()))));

            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.That(result, Is.EqualTo("UPDATE Person SET Age = (SELECT MAX(CarRef.Age) FROM Car CarRef)"));
        }

        [Test]
        public void ItShouldBePossibleToPerformAConditionalUpdate()
        {
            var stream =
                S.UpdateNode(
                    S.UpdHead<Person>(S.Ust<Person>("Age", 42)),
                    S.Where(S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Kalle'"))));

            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.That(result, Is.EqualTo("UPDATE Person SET Age = 42 WHERE Name = 'Kalle'"));
        }

        [Test]
        public void ItShouldBePossibleToHaveMultipleConditionsOnTheUpdate()
        {
            var stream =
                S.UpdateNode(
                    S.UpdHead<Person>(S.Ust<Person>("Age", 42)),
                    S.Where(
                        S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Kalle'")),
                        S.Or(S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Pelle'"))),
                        S.And(S.BinExp(S.Col<Person>("Age"), BinaryOperation.LessThan, S.Const(18)))));

            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.That(result, Is.EqualTo("UPDATE Person SET Age = 42 WHERE Name = 'Kalle' OR Name = 'Pelle' AND Age < 18"));
        }
    }
}
