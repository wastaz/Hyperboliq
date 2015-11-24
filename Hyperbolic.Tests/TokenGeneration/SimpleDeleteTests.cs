using Xunit;
using Hyperboliq.Domain;

namespace Hyperboliq.Tests.TokenGeneration
{
    [Trait("TokenGeneration", "Delete")]
    public class TokenGeneration_SimpleDeleteTests
    {
        [Fact]
        public void ItShouldBePossibleToGenerateADeleteAllRowsFromTableStatement()
        {
            var expr = Delete.From<Person>();
            var result = expr.ToSqlExpression();

            Assert.Equal(TokenGeneration_SimpleDeleteTests_Results.deleteAllRowsExpression, result);
        }

        [Fact]
        public void ItShouldBePossibleToPutAWhereExpressionOnADeleteStatement()
        {
            var expr = Delete.From<Person>().Where<Person>(p => p.Age > 42);
            var result = expr.ToSqlExpression();

            Assert.Equal(TokenGeneration_SimpleDeleteTests_Results.deleteWithWhereExpression, result);
        }

        [Fact]
        public void ItShouldBePossibleToUseAndAndOrOnAWhereStatementOnADeleteStatement()
        {
            var expr = Delete.From<Person>()
                             .Where<Person>(p => p.Age > 42)
                             .And<Person>(p => p.Name == "Kalle")
                             .Or<Person>(p => p.Name == "Henrik");
            var result = expr.ToSqlExpression();

            Assert.Equal(TokenGeneration_SimpleDeleteTests_Results.deleteWithWhereWithAndOrExpression, result);
        }

        [Fact]
        public void ItShouldBePossibleToDeleteWithASubQuery()
        {
            var expr = Delete.From<Car>()
                             .Where<Car>(c =>
                                Sql.In(
                                    c.DriverId,
                                    Select.Column<Person>(p => p.Id)
                                          .From<Person>()
                                          .Where<Person>(p => p.Age > 18)));
            var result = expr.ToSqlExpression();
            
            Assert.Equal(TokenGeneration_SimpleDeleteTests_Results.deleteWithSubQueryExpression, result);
        }
    }
}
