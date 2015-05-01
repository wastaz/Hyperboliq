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
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Select(Col<Person>("*")),
                    Kw(KeywordNode.From), 
                    Tbl<Person>());

            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBeAbleToSelectDistinctFromATable()
        {
            var expr = Select.Distinct.Star<Person>()
                             .From<Person>();
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    SelectDistinct(Col<Person>("*")),
                    Kw(KeywordNode.From),
                    Tbl<Person>());

            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBeAbleToSelectSingleColumnsFromATable()
        {
            var expr = Select.Column<Person>(p => new { p.Name, p.Age })
                             .From<Person>();
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Select(Col<Person>("Name"), Col<Person>("Age")),
                    Kw(KeywordNode.From),
                    Tbl<Person>());

            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBeAbleToSelectDistinctSingleColumnsFromATable()
        {
            var expr = Select.Distinct.Column<Person>(p => p.Age)
                             .From<Person>();
            var result = expr.ToSqlStream();


            var expected =
                StreamFrom(
                    SelectDistinct(Col<Person>("Age")),
                    Kw(KeywordNode.From),
                    Tbl<Person>());

            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBePossibleToSelectTheNumberOfRowsFromATable()
        {
            var expr = Select.Column<Person>(p => Sql.Count()).From<Person>();
            var result = expr.ToSqlStream();
            var expected =
                StreamFrom(
                    Select(Aggregate(AggregateType.Count)), 
                    Kw(KeywordNode.From),
                    Tbl<Person>());
            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBePossibleToSelectTheNumberOfRowsFromATableAndNameTheColumn()
        {
            var expr = Select.Column<Person>(p => new { NumberOfPersons = Sql.Count() }).From<Person>();
            var result = expr.ToSqlStream();
            var expected =
                StreamFrom(
                    Select(Aggregate(AggregateType.Count)),
                    Kw(KeywordNode.From),
                    Tbl<Person>());
            result.ShouldEqual(expected);
        }

    }
}
