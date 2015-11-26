using NUnit.Framework;
using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.AST.BinaryOperation;
using SqlGen = Hyperboliq.Domain.SqlGen;
using AggregateType = Hyperboliq.Domain.AST.AggregateType;

namespace Hyperboliq.Tests.SqlGeneration
{
    public class PersonLite
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    [TestFixture]
    public class SqlGeneration_CommonTableExpressions
    {
        [Test]
        public void ItShouldBePossibleToSelectFromACommonTableExpression()
        {
            var expr =
                S.SelectNode(
                    S.With(
                        S.TableDef<PersonLite>(
                            S.Select(S.Col<Person>("Name"), S.Col<Person>("Age")),
                            S.From<Person>(),
                            S.Where(S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(15))))),
                    S.Select(S.Col<PersonLite>("Name")),
                    S.From(new TableIdentifier<PersonLite>()),
                    S.Where(S.BinExp(S.Col<PersonLite>("Age"), BinaryOperation.Equal, S.Const(42)))
                );
            var result = SqlGen.SqlifyExpression(Dialects.AnsiSql.Dialect, expr);

            var expected =
                "WITH PersonLite AS ("+ 
                    "SELECT PersonRef.Name, PersonRef.Age " +
                    "FROM Person PersonRef " +
                    "WHERE PersonRef.Age > 15" +
                ") " +
                "SELECT PersonLiteRef.Name " +
                "FROM PersonLite PersonLiteRef " +
                "WHERE PersonLiteRef.Age = 42";

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBePossibleToSelectFromSeveralCommonTableExpressions()
        {
            var oldies = Table<PersonLite>.WithTableAlias("Oldies");
            var younglings = Table<PersonLite>.WithTableAlias("YoungOnes");

            var expr =
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

            var result = SqlGen.SqlifyExpression(Dialects.AnsiSql.Dialect, expr);

            var expected =
                "WITH Oldies AS (" +
                    "SELECT PersonRef.Name, PersonRef.Age " +
                    "FROM Person PersonRef " +
                    "WHERE PersonRef.Age > 40" +
                "), YoungOnes AS (" +
                    "SELECT PersonRef.Name, PersonRef.Age " +
                    "FROM Person PersonRef " +
                    "WHERE PersonRef.Age <= 15" +
                ") " +
                "SELECT OldiesRef.Name, YoungOnesRef.Name " +
                "FROM Oldies OldiesRef " +
                "INNER JOIN YoungOnes YoungOnesRef ON OldiesRef.Age - 30 = YoungOnesRef.Age";

            Assert.That(result, Is.EqualTo(expected));
        }

        public class PersonLitePagingResult
        {
            public int RowNumber { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }

        [Test]
        public void ItShouldBePossibleToDoPagingWithACommonTableExpression()
        {
            // To be honest, this case should be covered by the other test cases so this test case is a bit redundant.
            // However, using common table expressions for paging is a quite common technique and it's good to know for sure that it works as expected, so 
            // let's do some bad practice testing and test something that's already covered by other tests!

            var expr =
                S.SelectNode(
                    S.With(
                        S.TableDef<PersonLitePagingResult>(
                            S.Select(
                                S.Col<Person>("Name"),
                                S.Col<Person>("Age"),
                                S.AliasedCol(
                                    "RowNumber",
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

            var result = SqlGen.SqlifyExpression(Dialects.AnsiSql.Dialect, expr);

            var expected =
                "WITH PersonLitePagingResult AS (" +
                    "SELECT PersonRef.Name, PersonRef.Age, ROW_NUMBER() OVER (ORDER BY PersonRef.Age ASC) AS RowNumber " +
                    "FROM Person PersonRef" +
                ") " +
                "SELECT PersonLitePagingResultRef.Name, PersonLitePagingResultRef.Age " +
                "FROM PersonLitePagingResult PersonLitePagingResultRef " +
                "WHERE PersonLitePagingResultRef.RowNumber >= 10 AND PersonLitePagingResultRef.RowNumber < 20";

            Assert.That(result, Is.EqualTo(expected));
        }


        public class RecursivePerson
        {
            public int Level { get; set; }
            public string Name { get; set; }
            public int ParentId { get; set; }
        }

        [Test]
        public void ItShouldBePossibleToDoARecursiveCommonTableExpression()
        {
            var stream =
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
                                        S.AliasedCol(
                                            S.BinExp(S.Col<RecursivePerson>("Level"), BinaryOperation.Add, S.Const(1)), "Level"),
                                        S.Col<Person>("Name"),
                                        S.Col<Person>("ParentId")),
                                    S.From<Person>(
                                        S.Join<Person, RecursivePerson>(
                                            AST.JoinType.InnerJoin,
                                            S.BinExp(S.Col<Person>("Id"), BinaryOperation.Equal, S.Col<RecursivePerson>("ParentId")))))))),
                    S.PlainSelect(
                        S.Select(S.Star<RecursivePerson>()),
                        S.From<RecursivePerson>()));
            var result = SqlGen.SqlifyExpression(Dialects.AnsiSql.Dialect, stream);

            var expected =
                "WITH RecursivePerson AS (" +
                    "SELECT 0 AS Level, PersonRef.Name, PersonRef.ParentId FROM Person PersonRef WHERE PersonRef.Name = 'Kalle' " + 
                    "UNION ALL " +
                    "SELECT RecursivePersonRef.Level + 1 AS Level, PersonRef.Name, PersonRef.ParentId " +
                    "FROM Person PersonRef " +
                    "INNER JOIN RecursivePerson RecursivePersonRef ON PersonRef.Id = RecursivePersonRef.ParentId" +
                ") " +
                "SELECT RecursivePersonRef.* FROM RecursivePerson RecursivePersonRef";

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
