using Hyperboliq;
using Hyperboliq.Tests.Model;
using Xunit;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Stream;

namespace Hyperboliq.Tests
{
    [Trait("TokenGeneration", "Select")]
    public class TokenGeneration_SimpleSelectTests
    {
        [Fact]
        public void ItShouldBeAbleToSelectAllFromATable()
        {
            var expr = Select.Star<Person>()
                             .From<Person>();
            var result = expr.ToSqlExpression();

            var expected =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBeAbleToSelectDistinctFromATable()
        {
            var expr = Select.Distinct.Star<Person>()
                             .From<Person>();
            var result = expr.ToSqlExpression();

            var expected =
                SelectNode(
                    SelectDistinct(Col<Person>("*")),
                    From<Person>());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBeAbleToSelectSingleColumnsFromATable()
        {
            var expr = Select.Column<Person>(p => new { p.Name, p.Age })
                             .From<Person>();
            var result = expr.ToSqlExpression();

            var expected =
                SelectNode(
                    Select(Col<Person>("Name"), Col<Person>("Age")),
                    From<Person>());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBeAbleToSelectDistinctSingleColumnsFromATable()
        {
            var expr = Select.Distinct.Column<Person>(p => p.Age)
                             .From<Person>();
            var result = expr.ToSqlExpression();


            var expected =
                SelectNode(
                    SelectDistinct(Col<Person>("Age")),
                    From<Person>());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToSelectTheNumberOfRowsFromATable()
        {
            var expr = Select.Column<Person>(p => Sql.Count()).From<Person>();
            var result = expr.ToSqlExpression();
            var expected =
                SelectNode(
                    Select(Aggregate(AggregateType.Count)), 
                    From<Person>());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToSelectTheNumberOfRowsFromATableAndNameTheColumn()
        {
            var expr = Select.Column<Person>(p => new { NumberOfPersons = Sql.Count() }).From<Person>();
            var result = expr.ToSqlExpression();
            var expected =
                SelectNode(
                    Select(Aggregate(AggregateType.Count)),
                    From<Person>());
            Assert.Equal(expected, result);
        }

    }
}
