namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SimpleUpdateTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq.Tests.TokenGeneration.TestHelpers
    open Hyperboliq.Domain
    open Hyperboliq.Domain.AST
    open Hyperboliq
    open Hyperboliq.Tests.TokenGeneration.TokenGeneration_SimpleUpdateTests_Results

    [<Test>]
    let ``It should be possible to perform a global update`` () =
        let expr = Update<Person>.Set(<@ fun (p : Person) -> p.Name @>, "Kalle")
        let result = expr.ToSqlExpression()
        result |> should equal globalUpdateExpression

    [<Test>]
    let ``It should be possible to set multiple values in a single statement`` () = 
        let expr = Update<Person>.Set(<@ fun (p : Person) -> (p.Name, p.Age) @>, 
                                      ("Kalle", 42))
        let result = expr.ToSqlExpression()
        result |> should equal multipleSetsExpression

    [<Test>]
    let ``It should be possible to update columns in place`` () =
        let expr = Update<Person>.Set(<@ fun (p : Person) -> p.Age @>, <@ fun (p : Person) -> p.Age + 1 @>)
        let result = expr.ToSqlExpression()
        result |> should equal updateInPlaceExpression

    [<Test>]
    let ``It should be possible to update multiple columns in place`` () =
        let expr = Update<Person>.Set(<@ fun (p : Person) -> (p.Name, p.Age) @>, <@ fun (p : Person) -> ("Kalle" + p.Name, p.Age - 2 ) @>)
        let result = expr.ToSqlExpression()
        result |> should equal updateMultipleColumnsInPlaceExpression

    [<Test>]
    let ``It should be possible to update values to a subexpression`` () =
        let expr = Update<Person>.Set(<@ fun (p : Person) -> p.Age @>,
                                      Select.Column(<@ fun (c : Car) -> Sql.Max(c.Age) @>).From<Car>() :> ISelectExpressionTransformable)
        let result = expr.ToSqlExpression()
        result |> should equal updateValuesToSubExpression

    [<Test>]
    let ``It should be possible to perform a conditional update`` () =
        let expr = Update<Person>.Set(<@ fun (p : Person) -> p.Age @>, 42).Where(<@ fun (p : Person) -> p.Name = "Kalle" @>)
        let result = expr.ToSqlExpression()
        result |> should equal conditionalUpdateExpression

    [<Test>]
    let ``It should be possible to have multiple conditions on an update`` () =
        let expr = Update<Person>.Set(<@ fun (p : Person) -> p.Age @>, 42)
                                 .Where(<@ fun (p : Person) -> p.Name = "Kalle" @>)
                                 .Or(<@ fun (p : Person) -> p.Name = "Pelle" @>)
                                 .And(<@ fun (p : Person) -> p.Age < 18 @>)
        let result = expr.ToSqlExpression()
        result |> should equal multipleConditionsUpdateExpression