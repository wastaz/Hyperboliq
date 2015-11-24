namespace Hyperboliq.Tests.TokenGeneration

module SimpleOrderByTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq
    open Hyperboliq.Domain.AST
    open Hyperboliq.Domain
    open Hyperboliq.Tests.TokenGeneration.TokenGeneration_SimpleOrderByTests_Results

    let directions = 
      [ [| Direction.Ascending :> obj
           GetOrderByDirectionExpression(Direction.Ascending) :> obj |]
        [| Direction.Descending :> obj
           GetOrderByDirectionExpression(Direction.Descending) :> obj |] ] 

    [<Test>]
    [<TestCaseSource("directions")>]
    let ``It should be possible to specify the order direction`` direction expected =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .OrderBy(<@ fun (p : Person) -> p.Age @>, direction)
        let result = expr.ToSqlExpression()
        result |> should equal expected

    [<Test>]
    let ``It should be possible to order by several columns`` () =
        let expr = Select.Star<Person>().Star<Car>()
                         .From<Person>()
                         .InnerJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
                         .OrderBy(<@ fun (p : Person) -> p.Age @>, Direction.Ascending)
                         .ThenBy(<@ fun (c : Car) -> c.Brand @>, Direction.Descending)
        let result = expr.ToSqlExpression()
        result |> should equal orderByMultipleColumnsExpression


    // According to ANSI SQL the order by clause should support the keywords NULLS FIRST/NULLS LAST
    // after ASC/DESC in an ordering clause. However some DBMS'es does not implement this part of the standard...
    // But some do!
    let nullsOrdering = 
      [ [| NullsOrdering.NullsFirst :> obj
           GetOrderByNullsOrderingExpression(NullsOrdering.NullsFirst) :> obj |]
        [| NullsOrdering.NullsLast :> obj
           GetOrderByNullsOrderingExpression(NullsOrdering.NullsLast) :> obj |]
        [| NullsOrdering.NullsUndefined :> obj
           GetOrderByNullsOrderingExpression(NullsOrdering.NullsUndefined) :> obj |] ]

    [<Test>]
    [<TestCaseSource("nullsOrdering")>]
    let ``It should be possible to specify nulls ordering`` nullsOrder expected =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .OrderBy(<@ fun (p : Person) -> p.Age @>, Direction.Ascending, nullsOrder)
        let result = expr.ToSqlExpression()
        result |> should equal expected

    [<Test>]
    let ``It should be possible to specify different nulls ordering on each column`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .OrderBy(<@ fun (p : Person) -> p.Age @>, Direction.Ascending, NullsOrdering.NullsLast)
                         .ThenBy(<@ fun (p : Person) -> p.Name @>, Direction.Descending, NullsOrdering.NullsFirst)
        let result = expr.ToSqlExpression()
        result |> should equal orderByWithDifferentNullsOrderingPerColumnExpression