using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Hyperboliq.Tests.Model;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Domain;

namespace Hyperboliq.Tests.TokenGeneration
{
    [Trait("TokenGeneration", "SetOperations")]
    public class TokenGeneration_SetOperationTests
    {

        [Fact]
        public void ItCanDoASimpleUnion()
        {
            var expr =
                SetOperations.Union(
                    Select.Star<Person>()
                          .From<Person>()
                          .Where<Person>(p => p.Age > 42),
                    Select.Star<Person>()
                          .From<Person>()
                          .Where<Person>(p => p.Name == "Kalle")
                    );
            var result = expr.ToSqlExpression();

            var expected =
                Domain.Stream.SqlExpression.NewSelect(
                    Domain.Stream.SelectExpression.NewPlain(
                        Domain.Stream.PlainSelectExpression.NewSet(
                            S.Union(
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>(),
                                    S.Where(
                                        S.BinExp(S.Col<Person>("Age"), Domain.Stream.BinaryOperation.GreaterThan, S.Const(42)))),
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>(),
                                    S.Where(
                                        S.BinExp(S.Col<Person>("Name"), Domain.Stream.BinaryOperation.Equal, S.Const("'Kalle'"))))
                            ))));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItCanDoASimpleUnionAll()
        {
            var expr =
                SetOperations.UnionAll(
                    Select.Star<Person>()
                          .From<Person>()
                          .Where<Person>(p => p.Age > 42),
                    Select.Star<Person>()
                          .From<Person>()
                          .Where<Person>(p => p.Name == "Kalle")
                    );
            var result = expr.ToSqlExpression();

            var expected =
                Domain.Stream.SqlExpression.NewSelect(
                    Domain.Stream.SelectExpression.NewPlain(
                        Domain.Stream.PlainSelectExpression.NewSet(
                            S.UnionAll(
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>(),
                                    S.Where(
                                        S.BinExp(S.Col<Person>("Age"), Domain.Stream.BinaryOperation.GreaterThan, S.Const(42)))),
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>(),
                                    S.Where(
                                        S.BinExp(S.Col<Person>("Name"), Domain.Stream.BinaryOperation.Equal, S.Const("'Kalle'"))))
                            ))));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItCanDoASimpleIntersect()
        {
            var expr = SetOperations.Intersect(
                Select.Star<Person>()
                      .From<Person>(),
                Select.Star<Person>()
                      .From<Person>()
                      .Where<Person>(p => p.Age > 42));
            var result = expr.ToSqlExpression();

            var expected =
                Domain.Stream.SqlExpression.NewSelect(
                    Domain.Stream.SelectExpression.NewPlain(
                        Domain.Stream.PlainSelectExpression.NewSet(
                            S.Intersect(
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>()),
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>(),
                                    S.Where(
                                        S.BinExp(S.Col<Person>("Age"), Domain.Stream.BinaryOperation.GreaterThan, S.Const(42))))))));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItCanDoASimpleMinus()
        {
            var expr = SetOperations.Minus(
                Select.Star<Person>()
                      .From<Person>(),
                Select.Star<Person>()
                      .From<Person>()
                      .Where<Person>(p => p.Age > 42));
            var result = expr.ToSqlExpression();

            var expected =
                Domain.Stream.SqlExpression.NewSelect(
                    Domain.Stream.SelectExpression.NewPlain(
                        Domain.Stream.PlainSelectExpression.NewSet(
                            S.Minus(
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>()),
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>(),
                                    S.Where(
                                        S.BinExp(S.Col<Person>("Age"), Domain.Stream.BinaryOperation.GreaterThan, S.Const(42))))))));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItCanDoAUnionsWithCommonTableExpressions()
        {
            
            var identifier = Table<Person>.WithTableAlias("cte");
            var expr =
                With.Table(
                        identifier,
                        SetOperations.Union(
                            Select.Star<Person>().From<Person>().Where<Person>(p => p.Age > 42),
                            Select.Star<Person>().From<Person>().Where<Person>(p => p.Name == "Kalle")))
                    .Query(
                        SetOperations.Union(
                            Select.Star<Person>().From(identifier).Where<Person>(p => p.Age == 50),
                            Select.Star<Person>().From(identifier).Where<Person>(p => p.Name == "Kalle")));

            var result = expr.ToSqlExpression();

            var union =
                S.Union(
                    S.PlainSelect(
                        S.Select(S.Star<Person>()),
                        S.From(identifier),
                        S.Where(
                            S.BinExp(S.Col<Person>("Age"), Domain.Stream.BinaryOperation.Equal, S.Const(50)))),
                    S.PlainSelect(
                        S.Select(S.Star<Person>()),
                        S.From(identifier),
                        S.Where(
                            S.BinExp(S.Col<Person>("Name"), Domain.Stream.BinaryOperation.Equal, S.Const("'Kalle'")))));
            var with =
                    S.With(
                        S.TableDef(
                            identifier,
                            S.Union(
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>(),
                                    S.Where(
                                        S.BinExp(S.Col<Person>("Age"), Domain.Stream.BinaryOperation.GreaterThan, S.Const(42)))),
                                S.PlainSelect(
                                    S.Select(S.Star<Person>()),
                                    S.From<Person>(),
                                    S.Where(
                                        S.BinExp(S.Col<Person>("Name"), Domain.Stream.BinaryOperation.Equal, S.Const("'Kalle'")))))));
            var expected = S.SelectNode(with, union);
            Assert.Equal(expected, result);
        }
    }
}
