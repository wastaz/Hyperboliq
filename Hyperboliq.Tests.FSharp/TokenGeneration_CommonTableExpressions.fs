namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_CommonTableExpressions =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq.Tests.TokenGeneration.TestHelpers
    open Hyperboliq
    open Hyperboliq.Domain
    open Hyperboliq.Domain.AST
    open Hyperboliq.Tests.TokenGeneration.TokenGeneration_CommonTableExpressions_Results

    [<Test>]
    let ``It should be possible to select from a common table expression`` () =
        let expr = 
            With.Table<PersonLite>(
                    Select.Column(<@ fun (p : Person) -> p.Name, p.Age @>)
                          .From<Person>()
                          .Where(<@ fun (p : Person) -> p.Age > 15 @>))
                .Query(
                    Select.Column(<@ fun (p : PersonLite) -> p.Name @>)
                          .From<PersonLite>()
                          .Where(<@ fun (p : PersonLite) -> p.Age = 42 @>))
        let result = expr.ToSqlExpression()
        result |> should equal selectFromACteExpression

    [<Test>]
    let ``It should be possible to select from several common table expressions`` () =
        let oldies = Table<PersonLite>.WithTableAlias("Oldies")
        let younglings = Table<PersonLite>.WithTableAlias("YoungOnes")

        let expr =
            With.Table(
                    oldies,
                    Select.Column(<@ fun (p : Person) -> p.Name, p.Age @>)
                          .From<Person>()
                          .Where(<@ fun (p : Person) -> p.Age > 40 @>))
                .Table(
                    younglings,
                    Select.Column(<@ fun (p : Person) -> p.Name, p.Age @>)
                          .From<Person>()
                          .Where(<@ fun (p : Person) -> p.Age <= 15 @>))
                .Query(
                    Select.Column(oldies, <@ fun (p : PersonLite) -> p.Name @>)
                          .Column(younglings, <@ fun (p : PersonLite) -> p.Name @>)
                          .From(oldies)
                          .InnerJoin(oldies, younglings, <@ fun (old : PersonLite) (young : PersonLite) -> old.Age - 30 = young.Age @>))
        let result = expr.ToSqlExpression()
        result |> should equal selectFromSeveralCtesExpression

    [<Test>]
    let ``It should be possible to do paging with a common table expression`` () =
        // To be honest, this case should be covered by the other test cases so this test case is a bit redundant.
        // However, using common table expressions for paging is a quite common technique and it's good to know for sure that it works as expected, so 
        // let's do some bad practice testing and test something that's already covered by other tests!

        let expr =
            With.Table<PersonLitePagingResult>(
                Select.Column(<@ fun (p : Person) -> Sql.RowNumber() @>, Over.OrderBy(<@ fun (p : Person) -> p.Age @>))
                      .Column(<@ fun (p : Person) -> p.Name, p.Age @>)
                      .From<Person>()
            ).Query(
                Select.Column(<@ fun (p : PersonLitePagingResult) -> p.Name, p.Age @>)
                      .From<PersonLitePagingResult>()
                      .Where(<@ fun (p : PersonLitePagingResult) -> p.RowNumber >= 10 && p.RowNumber < 20 @>)
            )
        let result = expr.ToSqlExpression()
        result |> should equal commonPagingExpression

    [<Test>]
    let ``It should be possible to do a recursive common table expression`` () =
        let expr =
            With.Table<RecursivePerson>(
                SetOperations.UnionAll(
                    Select.Column(<@ fun (p : Person) -> let Level = 0 in (Level, p.Name, p.ParentId) @>)
                          .From<Person>()
                          .Where(<@ fun (p : Person) -> p.Name = "Kalle" @>),
                    Select.Column(<@ fun (rp : RecursivePerson) -> let Level = rp.Level + 1 in Level @>)
                          .Column(<@ fun (p : Person) -> p.Name, p.ParentId @>)
                          .From<Person>()
                          .InnerJoin(<@ fun (p : Person) (rp : RecursivePerson) -> p.Id = rp.ParentId @>)))
                .Query(Select.Star<RecursivePerson>()
                             .From<RecursivePerson>());
        let result = expr.ToSqlExpression()
        result |> should equal recursiveCommonTableExpression
        