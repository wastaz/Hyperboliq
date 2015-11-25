using Xunit;
using Hyperboliq.Domain;

namespace Hyperboliq.Tests.TokenGeneration
{
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
            Assert.Equal(TokenGeneration_CommonTableExpressions_Results.selectFromACteExpression, result);
        }

        [Fact]
        public void ItShouldBePossibleToSelectFromSeveralCommonTableExpressions()
        {
            var oldies = Table<PersonLite>.WithTableAlias("Oldies");
            var younglings = Table<PersonLite>.WithTableAlias("YoungOnes");

            var expr =
                With.Table(
                        oldies,
                        Select.Column<Person>(p => new { p.Name, p.Age, })
                              .From<Person>()
                              .Where<Person>(p => p.Age > 40))
                    .Table(
                        younglings,
                        Select.Column<Person>(p => new { p.Name, p.Age, })
                              .From<Person>()
                              .Where<Person>(p => p.Age <= 15))
                    .Query(
                        Select.Column(oldies, p => p.Name)
                              .Column(younglings, p => p.Name)
                              .From(oldies)
                              .InnerJoin(oldies, younglings, (old, young) => old.Age - 30 == young.Age));
            var result = expr.ToSqlExpression();
            Assert.Equal(TokenGeneration_CommonTableExpressions_Results.selectFromSeveralCtesExpression, result);
        }
        
        [Fact]
        public void ItShouldBePossibleToDoPagingWithACommonTableExpression()
        {
            // To be honest, this case should be covered by the other test cases so this test case is a bit redundant.
            // However, using common table expressions for paging is a quite common technique and it's good to know for sure that it works as expected, so 
            // let's do some bad practice testing and test something that's already covered by other tests!

            var expr =
                With.Table<PersonLitePagingResult>(
                    Select.Column<Person>(p => Sql.RowNumber(), Over.OrderBy<Person>(p => p.Age))
                          .Column<Person>(p => new { p.Name, p.Age })
                          .From<Person>()
                ).Query(
                    Select.Column<PersonLitePagingResult>(p => new { p.Name, p.Age })
                          .From<PersonLitePagingResult>()
                          .Where<PersonLitePagingResult>(p => p.RowNumber >= 10 && p.RowNumber < 20)
                );
            var result = expr.ToSqlExpression();
            Assert.Equal(TokenGeneration_CommonTableExpressions_Results.commonPagingExpression, result);
        }

        [Fact]
        public void ItShouldBePossibleToDoARecursiveCommonTableExpression()
        {
            var expr =
                With.Table<RecursivePerson>(
                    SetOperations.UnionAll(
                        Select.Column<Person>(p => new { Level = 0, p.Name, p.ParentId, })
                              .From<Person>()
                              .Where<Person>(p => p.Name == "Kalle"),
                        Select.Column<RecursivePerson>(rp => new { Level = rp.Level + 1 })
                              .Column<Person>(p => new { p.Name, p.ParentId })
                              .From<Person>()
                              .InnerJoin<Person, RecursivePerson>((p, rp) => p.Id == rp.ParentId)))
                    .Query(Select.Star<RecursivePerson>().From<RecursivePerson>());
            var result = expr.ToSqlExpression();
            Assert.Equal(TokenGeneration_CommonTableExpressions_Results.recursiveCommonTableExpression, result);
        }
    }
}
