using NUnit.Framework;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Domain;
using Hyperboliq.Dialects;
using Hyperboliq.Tests.TokenGeneration;

namespace Hyperboliq.Tests.SqlGeneration
{
    [TestFixture]
    public class SqlGeneration_SetOperationTests
    {
        [Test]
        public void ItShouldHandleASimpleUnion()
        {
            var stream =
                AST.SqlExpression.NewSelect(
                    AST.SelectExpression.NewPlain(
                        AST.PlainSelectExpression.NewSet(
                            S.Union(
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>(),
                                    S.Where(
                                        S.BinExp(S.Col<Person>("Age"), AST.BinaryOperation.GreaterThan, S.Const(42)))),
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>(),
                                    S.Where(
                                        S.BinExp(S.Col<Person>("Name"), AST.BinaryOperation.Equal, S.Const("'Kalle'"))))
                            ))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            var expected = 
                "SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age > 42 " +
                "UNION " +
                "SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Name = 'Kalle'";
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldHandleASimpleUnionAll()
        {
            var stream =
               AST.SqlExpression.NewSelect(
                   AST.SelectExpression.NewPlain(
                       AST.PlainSelectExpression.NewSet(
                           S.UnionAll(
                               S.PlainSelect(
                                   S.Select(S.Star<Person>()),
                                   S.From<Person>(),
                                   S.Where(
                                       S.BinExp(S.Col<Person>("Age"), AST.BinaryOperation.GreaterThan, S.Const(42)))),
                               S.PlainSelect(
                                   S.Select(S.Star<Person>()),
                                   S.From<Person>(),
                                   S.Where(
                                       S.BinExp(S.Col<Person>("Name"), AST.BinaryOperation.Equal, S.Const("'Kalle'"))))
                           ))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            var expected =
                "SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age > 42 " +
                "UNION ALL " +
                "SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Name = 'Kalle'";
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldHandleASimpleIntersect()
        {
            var stream =
                AST.SqlExpression.NewSelect(
                    AST.SelectExpression.NewPlain(
                        AST.PlainSelectExpression.NewSet(
                            S.Intersect(
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>()),
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>(),
                                    S.Where(
                                        S.BinExp(S.Col<Person>("Age"), AST.BinaryOperation.GreaterThan, S.Const(42))))))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            var expected =
                "SELECT PersonRef.* FROM Person PersonRef " +
                "INTERSECT " +
                "SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age > 42";
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldHandleASimpleMinus()
        {
            var stream =
                AST.SqlExpression.NewSelect(
                    AST.SelectExpression.NewPlain(
                        AST.PlainSelectExpression.NewSet(
                            S.Minus(
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>()),
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>(),
                                    S.Where(
                                        S.BinExp(S.Col<Person>("Age"), AST.BinaryOperation.GreaterThan, S.Const(42))))))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            var expected =
                "SELECT PersonRef.* FROM Person PersonRef " +
                "MINUS " +
                "SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age > 42";
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
