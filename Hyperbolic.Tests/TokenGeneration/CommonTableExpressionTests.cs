using Xunit;
using Hyperboliq.Tests.Model;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.Stream.BinaryOperation;

namespace Hyperboliq.Tests.TokenGeneration
{
    public class PersonLite
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    [Trait("TokenGeneration", "CommonTableExpressions")]
    public class CommonTableExpressionTests
    {
        [Fact]
        public void ItShouldBePossibleToSelectFromACommonTableExpression() 
        {
            var expr =
                With.Table<PersonLite>(
                        Select.Column<Person>(p => new { p.Name, p.Age })
                              .From<Person>()
                              .Where<Person>(p => p.Age > 15))
                    .Query(
                        Select.Column<PersonLite>(p => p.Name)
                              .From<PersonLite>()
                              .Where<PersonLite>(p => p.Age == 42));
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.With(
                        S.TableDef<PersonLite>(
                            S.Select(S.Col<Person>("Name"), S.Col<Person>("Age")),
                            S.From<Person>(),
                            S.Where(S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(15))))),
                    S.Select(S.Col<PersonLite>("Name")),
                    S.From<PersonLite>(),
                    S.Where(S.BinExp(S.Col<PersonLite>("Age"), BinaryOperation.Equal, S.Const(42)))
                );

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToSelectFromSeveralCommonTableExpressions()
        {
            var oldies = Table<PersonLite>.WithTableAlias("Oldies");
            var younglings = Table<PersonLite>.WithTableAlias("YoungOnes");

            var expr =
                With.Table(
                        oldies,
                        Select.Column<Person>(p => new { p.Name, p.Age, })
                              .From<Person>()
                              .Where<Person>(p => p.Age > 40))
                    .Table(
                        younglings,
                        Select.Column<Person>(p => new { p.Name, p.Age, })
                              .From<Person>()
                              .Where<Person>(p => p.Age <= 15))
                    .Query(
                        Select.Column(oldies, p => p.Name)
                              .Column(younglings, p => p.Name)
                              .From(oldies)
                              .InnerJoin(oldies, younglings, (old, young) => old.Age - 30 == young.Age));
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.With(
                        S.TableDef(
                            younglings,
                            S.Select(S.Col<Person>("Name"), S.Col<Person>("Age")),
                            S.From<Person>(),
                            S.Where(S.BinExp(S.Col<Person>("Age"), BinaryOperation.LessThanOrEqual, S.Const(15)))),
                        S.TableDef(
                            oldies,
                            S.Select(S.Col<Person>("Name"), S.Col<Person>("Age")),
                            S.From<Person>(),
                            S.Where(S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(40))))
                    ),
                    S.Select(S.Col(oldies, "Name"), S.Col(younglings, "Name")),
                    S.From(
                        oldies,
                        S.Join(
                            oldies, 
                            younglings, 
                            Domain.Stream.JoinType.InnerJoin, 
                            S.BinExp(
                                S.BinExp(S.Col(oldies, "Age"), BinaryOperation.Subtract, S.Const(30)),
                                BinaryOperation.Equal,
                                S.Col(younglings, "Age"))))
                );

            Assert.Equal(expected, result);
        }
    }
}
