using Xunit;
using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;

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

            Assert.Equal(TokenGeneration_SimpleSelectTests_Results.selectDistinctExpression, result);
        }

        [Fact]
        public void ItShouldBePossibleToSelectAConstant()
        {
            var expr = Select.Column<Person>(p => new { favoriteNumber = 42, name = p.Name })
                             .From<Person>();
            var result = expr.ToSqlExpression();

            Assert.Equal(TokenGeneration_SimpleSelectTests_Results.selectConstantExpression, result);
        }

        [Fact]
        public void ItShouldBeAbleToSelectSingleColumnsFromATable()
        {
            var expr = Select.Column<Person>(p => new { p.Name, p.Age })
                             .From<Person>();
            var result = expr.ToSqlExpression();

            Assert.Equal(TokenGeneration_SimpleSelectTests_Results.selectColumnsExpression, result);
        }

        [Fact]
        public void ItShouldOrderTheColumnsInTheExceptedOrderWhenCallingColumnSeveralTimes()
        {
            var expr = Select.Column<Person>(p => p.Name).Column<Person>(p => p.Age)
                             .From<Person>();
            var result = expr.ToSqlExpression();

            Assert.Equal(TokenGeneration_SimpleSelectTests_Results.selectColumnsExpression, result);
        }

        [Fact]
        public void ItShouldBeAbleToSelectDistinctSingleColumnsFromATable()
        {
            var expr = Select.Distinct.Column<Person>(p => p.Age)
                             .From<Person>();
            var result = expr.ToSqlExpression();

            Assert.Equal(TokenGeneration_SimpleSelectTests_Results.selectDistinctSingleColumnsExpression, result);
        }

        [Fact]
        public void ItShouldBePossibleToSelectTheNumberOfRowsFromATable()
        {
            var expr = Select.Column<Person>(p => Sql.Count()).From<Person>();
            var result = expr.ToSqlExpression();

            Assert.Equal(TokenGeneration_SimpleSelectTests_Results.selectCountExpression, result);
        }

        [Fact]
        public void ItShouldBePossibleToSelectTheNumberOfRowsFromATableAndNameTheColumn()
        {
            var expr = Select.Column<Person>(p => new { numberOfPersons = Sql.Count() }).From<Person>();
            var result = expr.ToSqlExpression();

            Assert.Equal(TokenGeneration_SimpleSelectTests_Results.selectNamedCountExpression, result);
        }

    }
}
