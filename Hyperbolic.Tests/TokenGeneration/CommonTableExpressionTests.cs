using Xunit;
using Hyperboliq.Tests.Model;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.Types.BinaryOperation;

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
    }
}
