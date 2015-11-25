namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SubExpressionTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq
    open Hyperboliq.Domain
    open Hyperboliq.Tests.TokenGeneration.TokenGeneration_SubExpression_Results

    [<Test>]
    let ``It should be possible to compare against a subexpression in a where expression`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .Where(<@ fun (p : Person) -> p.Age > Sql.SubExpr(Select.Column(<@ fun (c : Car) -> c.Age @>)
                                                                                 .From<Car>()
                                                                                 .Where(<@ fun (c : Car) -> c.Id = 42 @>)) @>)
        let result = expr.ToSqlExpression()
        result |> should equal compareAgainstSubExprInWhereExpression

    [<Test>]
    let ``It should be possible to do an in query with a subexpression`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .Where(<@ fun (p : Person) -> Sql.In(p.Id, Select.Column(<@ fun (c : Car) -> c.DriverId @>).From<Car>()) @>)
        let result = expr.ToSqlExpression()
        result |> should equal subExprInInExpression