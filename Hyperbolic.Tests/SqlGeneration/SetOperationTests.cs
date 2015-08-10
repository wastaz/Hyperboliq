using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Hyperboliq.Tests.Model;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Domain;
using Hyperboliq.Dialects;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "SetOperations")]
    public class SqlGeneration_SetOperationTests
    {
        [Fact]
        public void ItShouldHandleASimpleUnion()
        {
            var stream =
                Stream.SqlExpression.NewSelect(
                    Stream.SelectExpression.NewPlain(
                        Stream.PlainSelectExpression.NewSet(
                            S.Union(
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>(),
                                    S.Where(
                                        S.BinExp(S.Col<Person>("Age"), Stream.BinaryOperation.GreaterThan, S.Const(42)))),
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>(),
                                    S.Where(
                                        S.BinExp(S.Col<Person>("Name"), Stream.BinaryOperation.Equal, S.Const("'Kalle'"))))
                            ))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            var expected = 
                "SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age > 42 " +
                "UNION " +
                "SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Name = 'Kalle'";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldHandleASimpleIntersect()
        {
            var stream =
                Stream.SqlExpression.NewSelect(
                    Stream.SelectExpression.NewPlain(
                        Stream.PlainSelectExpression.NewSet(
                            S.Intersect(
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>()),
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>(),
                                    S.Where(
                                        S.BinExp(S.Col<Person>("Age"), Stream.BinaryOperation.GreaterThan, S.Const(42))))))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            var expected =
                "SELECT PersonRef.* FROM Person PersonRef " +
                "INTERSECT " +
                "SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age > 42";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldHandleASimpleMinus()
        {
            var stream =
                Stream.SqlExpression.NewSelect(
                    Stream.SelectExpression.NewPlain(
                        Stream.PlainSelectExpression.NewSet(
                            S.Minus(
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>()),
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>(),
                                    S.Where(
                                        S.BinExp(S.Col<Person>("Age"), Stream.BinaryOperation.GreaterThan, S.Const(42))))))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            var expected =
                "SELECT PersonRef.* FROM Person PersonRef " +
                "MINUS " +
                "SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age > 42";
            Assert.Equal(expected, result);
        }
    }
}
