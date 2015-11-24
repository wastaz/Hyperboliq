namespace Hyperboliq.Tests.TokenGeneration

module SimpleDeleteTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq
    open Hyperboliq.Domain.AST
    open Hyperboliq.Domain
    open Hyperboliq.Tests.TokenGeneration.TokenGeneration_SimpleDeleteTests_Results

    [<Test>]
    let ``It should be possible to generate a delete all rows from table statement`` () =
        let expr = Delete.From<Person>()
        let result = expr.ToSqlExpression()

        result |> should equal deleteAllRowsExpression

    [<Test>]
    let ``It should be possible to put a where expression on a delete statement`` () =
        let expr = Delete.From<Person>()
                         .Where(<@ fun (p : Person) -> p.Age > 42 @>)
        let result = expr.ToSqlExpression()
        
        result |> should equal deleteWithWhereExpression

    [<Test>]
    let ``It should be possible to use and and or on a where statement in a delete statement`` () =
        let expr = Delete.From<Person>()
                         .Where(<@ fun (p : Person) -> p.Age > 42@>)
                         .And(<@ fun (p : Person) -> p.Name = "Kalle" @>)
                         .Or(<@ fun (p : Person) -> p.Name = "Henrik" @>)
        let result = expr.ToSqlExpression()

        result |> should equal deleteWithWhereWithAndOrExpression

    [<Test>]
    let ``It should be possible to delete with a subquery`` () =
        let expr = Delete.From<Car>()
                         .Where(<@ fun (c : Car) ->
                                Sql.In(
                                    c.DriverId, 
                                    Select.Column(<@ fun (p : Person) -> p.Id @>)
                                          .From<Person>()
                                          .Where(<@ fun (p : Person) -> p.Age > 18 @>)) @>)
        let result = expr.ToSqlExpression()

        result |> should equal deleteWithSubQueryExpression