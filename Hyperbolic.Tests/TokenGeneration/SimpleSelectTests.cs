using Xunit;
using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using AggregateType = Hyperboliq.Domain.AST.AggregateType;
using ValueNode = Hyperboliq.Domain.AST.ValueNode;

namespace Hyperboliq.Tests
{
    [Trait("TokenGeneration", "Select")]
    public class TokenGeneration_SimpleSelectTests
    {
        [Fact]
        public void ItShouldBeAbleToSelectDistinctFromATable()
        {
            var expr = Select.Distinct.Star<Person>()
                             .From<Person>();
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.SelectDistinct(S.Star<Person>()),
                    S.From<Person>());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToSelectAConstant()
        {
            var expr = Select.Column<Person>(p => new { FavoriteNumber = 42, Name = p.Name })
                             .From<Person>();
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(S.AliasedCol(S.Const(42), "FavoriteNumber"), S.Col<Person>("Name")),
                    S.From<Person>());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBeAbleToSelectSingleColumnsFromATable()
        {
            var expr = Select.Column<Person>(p => new { p.Name, p.Age })
                             .From<Person>();
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(S.Col<Person>("Name"), S.Col<Person>("Age")),
                    S.From<Person>());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldOrderTheColumnsInTheExceptedOrderWhenCallingColumnSeveralTimes()
        {
            var expr = Select.Column<Person>(p => p.Name).Column<Person>(p => p.Age)
                             .From<Person>();
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(S.Col<Person>("Name"), S.Col<Person>("Age")),
                    S.From<Person>());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBeAbleToSelectDistinctSingleColumnsFromATable()
        {
            var expr = Select.Distinct.Column<Person>(p => p.Age)
                             .From<Person>();
            var result = expr.ToSqlExpression();


            var expected =
                S.SelectNode(
                    S.SelectDistinct(S.Col<Person>("Age")),
                    S.From<Person>());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToSelectTheNumberOfRowsFromATable()
        {
            var expr = Select.Column<Person>(p => Sql.Count()).From<Person>();
            var result = expr.ToSqlExpression();
            var expected =
                S.SelectNode(
                    S.Select(S.Aggregate(AggregateType.Count, ValueNode.NullValue)), 
                    S.From<Person>());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToSelectTheNumberOfRowsFromATableAndNameTheColumn()
        {
            var expr = Select.Column<Person>(p => new { NumberOfPersons = Sql.Count() }).From<Person>();
            var result = expr.ToSqlExpression();
            var expected =
                S.SelectNode(
                    S.Select(
                        S.AliasedCol<Person>(
                            AggregateType.Count, 
                            ValueNode.NullValue,
                            "NumberOfPersons")),
                    S.From<Person>());
            Assert.Equal(expected, result);
        }

    }
}
