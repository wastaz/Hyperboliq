using Xunit;
using Hyperboliq.Tests.Model;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.Stream.BinaryOperation;
using SqlGen = Hyperboliq.Domain.SqlGen;

namespace Hyperboliq.Tests.SqlGeneration
{
    public class PersonLite
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    [Trait("SqlGeneration", "CommonTableExpressions")]
    public class SqlGeneration_CommonTableExpressions
    {
        [Fact]
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

            Assert.Equal(expected, result);
        }

        [Fact]
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
                            Domain.Stream.JoinType.InnerJoin,
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

            Assert.Equal(expected, result);
        }
    }
}
