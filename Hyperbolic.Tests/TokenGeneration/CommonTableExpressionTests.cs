using Xunit;
using Hyperboliq.Domain;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.AST.BinaryOperation;
using AggregateType = Hyperboliq.Domain.AST.AggregateType;

namespace Hyperboliq.Tests.TokenGeneration
{
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
                            Domain.AST.JoinType.InnerJoin, 
                            S.BinExp(
                                S.BinExp(S.Col(oldies, "Age"), BinaryOperation.Subtract, S.Const(30)),
                                BinaryOperation.Equal,
                                S.Col(younglings, "Age"))))
                );

            Assert.Equal(expected, result);
        }


        public class PersonLitePagingResult
        {
            public int RowNumber { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }

        [Fact]
        public void ItShouldBePossibleToDoPagingWithACommonTableExpression()
        {
            // To be honest, this case should be covered by the other test cases so this test case is a bit redundant.
            // However, using common table expressions for paging is a quite common technique and it's good to know for sure that it works as expected, so 
            // let's do some bad practice testing and test something that's already covered by other tests!

            var expr =
                With.Table<PersonLitePagingResult>(
                    Select.Column<Person>(p => Sql.RowNumber(), Over.OrderBy<Person>(p => p.Age))
                          .Column<Person>(p => new { p.Name, p.Age })
                          .From<Person>()
                ).Query(
                    Select.Column<PersonLitePagingResult>(p => new { p.Name, p.Age })
                          .From<PersonLitePagingResult>()
                          .Where<PersonLitePagingResult>(p => p.RowNumber >= 10 && p.RowNumber < 20)
                );
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.With(
                        S.TableDef<PersonLitePagingResult>(
                            S.Select(
                                S.Col<Person>("Name"),
                                S.Col<Person>("Age"),
                                S.WinCol(
                                    AggregateType.RowNumber,
                                    S.Null(),
                                    orderBy: new[] { S.OrderClause(S.Col<Person>("Age"), AST.Direction.Ascending) })),
                            S.From<Person>())
                    ),
                    S.Select(S.Col<PersonLitePagingResult>("Name"), S.Col<PersonLitePagingResult>("Age")),
                    S.From<PersonLitePagingResult>(),
                    S.Where(
                        S.BinExp(
                            S.BinExp(S.Col<PersonLitePagingResult>("RowNumber"), BinaryOperation.GreaterThanOrEqual, S.Const(10)),
                            BinaryOperation.And,
                            S.BinExp(S.Col<PersonLitePagingResult>("RowNumber"), BinaryOperation.LessThan, S.Const(20))
                    )));

            Assert.Equal(expected, result);
        }


        public class RecursivePerson
        {
            public int Level { get; set; }
            public string Name { get; set; }
            public int ParentId { get; set; }
        }

        [Fact]
        public void ItShouldBePossibleToDoARecursiveCommonTableExpression()
        {
            var expr =
                With.Table<RecursivePerson>(
                    SetOperations.UnionAll(
                        Select.Column<Person>(p => new { Level = 0, p.Name, p.ParentId, })
                              .From<Person>()
                              .Where<Person>(p => p.Name == "Kalle"),
                        Select.Column<RecursivePerson>(rp => new { Level = rp.Level + 1 })
                              .Column<Person>(p => new { p.Name, p.ParentId })
                              .From<Person>()
                              .InnerJoin<Person, RecursivePerson>((p, rp) => p.Id == rp.ParentId)))
                    .Query(Select.Star<RecursivePerson>().From<RecursivePerson>());
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.With(
                        S.TableDef<RecursivePerson>(
                            S.UnionAll(
                                S.PlainSelect(
                                    S.Select(
                                        S.AliasedCol(S.Const(0), "Level"),
                                        S.Col<Person>("Name"),
                                        S.Col<Person>("ParentId")),
                                    S.From<Person>(),
                                    S.Where(
                                        S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Kalle'")))),
                                S.PlainSelect(
                                    S.Select(
                                        S.Col<Person>("Name"),
                                        S.Col<Person>("ParentId"), 
                                        S.AliasedCol(S.BinExp(S.Col<RecursivePerson>("Level"), BinaryOperation.Add, S.Const(1)), "Level")),
                                    S.From<Person>(
                                        S.Join<Person, RecursivePerson>(
                                            AST.JoinType.InnerJoin,
                                            S.BinExp(S.Col<Person>("Id"), BinaryOperation.Equal, S.Col<RecursivePerson>("ParentId")))))))),
                    S.PlainSelect(
                        S.Select(S.Star<RecursivePerson>()),
                        S.From<RecursivePerson>()));

            Assert.Equal(expected, result);
        }
    }
}
