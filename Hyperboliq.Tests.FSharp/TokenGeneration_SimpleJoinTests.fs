namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SimpleJoinTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq
    open Hyperboliq.Domain.AST
    open Hyperboliq.Domain
    open Hyperboliq.Tests.TokenGeneration.TokenGeneration_SimpleJoinTests_Results

    [<Test>]
    let ``It should be able to perform a simple inner join`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .InnerJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
        let result = expr.ToSqlExpression()
        let expected = expectedAst JoinType.InnerJoin
        result |> should equal expected

    [<Test>]
    let ``It should be able to perform a simple left join`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .LeftJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
        let result = expr.ToSqlExpression()
        let expected = expectedAst JoinType.LeftJoin
        result |> should equal expected

    [<Test>]
    let ``It should be able to perform a simple right join`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .RightJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
        let result = expr.ToSqlExpression()
        let expected = expectedAst JoinType.RightJoin
        result |> should equal expected
    
    [<Test>]
    let ``It should be able to perform a simple full outer join`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .FullJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
        let result = expr.ToSqlExpression()
        let expected = expectedAst JoinType.FullJoin
        result |> should equal expected
        
    [<Test>]
    let ``It should be able to perform multiple joins`` () =
        let expr = Select.Star<Person>().Star<Car>().Star<House>()
                         .From<House>()
                         .InnerJoin(<@ fun (h : House) (p : Person) -> h.Id = p.LivesAtHouseId @>)
                         .LeftJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
        let result = expr.ToSqlExpression()

        result |> should equal multipleJoinExpression