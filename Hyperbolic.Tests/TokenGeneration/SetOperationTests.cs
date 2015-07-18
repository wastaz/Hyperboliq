using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Hyperboliq.Tests.Model;
using S = Hyperboliq.Tests.SqlStreamExtensions;

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
                    Domain.Stream.SelectExpression.NewSet(
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
                        )));

            Assert.Equal(expected, result);
        }
    }
}
