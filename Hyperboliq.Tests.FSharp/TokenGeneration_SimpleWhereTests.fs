namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SimpleWhereTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq.Tests.TokenGeneration.TestHelpers
    open Hyperboliq
    open Hyperboliq.Domain
    open Hyperboliq.Domain.AST
    open Hyperboliq.Tests.TokenGeneration.TokenGeneration_SimpleWhereTests_Results

    [<Test>]
    let ``It should handle a simple where condition`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .Where(<@ fun (p : Person) -> p.Age > 42 @>)
        let result = expr.ToSqlExpression()
        result |> should equal simpleWhereConditionExpression

    [<Test>]
    let ``It should handle a conditional with and and ors in the expression`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .Where(<@ fun (p : Person) -> p.Age > 42 || (p.Age < 10 && p.Name = "Karl") @>)
        let result = expr.ToSqlExpression()
        result |> should equal conditionalWithAndAndOrsExpression

    [<Test>]
    let ``It should handle a where condition with and and ors that are not in the expression`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .Where(<@ fun (p : Person) -> p.Age < 42 @>)
                         .And(<@ fun (p : Person) -> p.Age > 12 @>)
                         .Or(<@ fun (p : Person) -> p.Name = "Karl" @>)
        let result = expr.ToSqlExpression()
        result |> should equal conditionalWithAndAndOrsOutsideExpression

    [<Test>]
    let ``It should be possible to make where conditions on joined tables`` () =
        let expr = Select.Star<Person>().Star<Car>()
                         .From<Person>()
                         .InnerJoin<Person, Car>(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
                         .Where(<@ fun (p : Person) -> p.Age > 42 @>)
                         .And(<@ fun (c : Car) -> c.Brand = "SAAB" @>)
        let result = expr.ToSqlExpression()
        result |> should equal conditionsOnJoinedTablesExpression

    [<Test>]
    let ``It should be possible to reference several tables in a where expression`` () =
        let expr = Select.Star<Person>().Star<Car>()
                         .From<Person>()
                         .InnerJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
                         .Where(<@ fun (p : Person) (c : Car) -> p.Age > c.DriverId @>)
        let result = expr.ToSqlExpression()
        result |> should equal conditionalReferencingSeveralTablesExpression

    [<Test>]
    let ``It should be possible to reference several tables in a where condition with and and or`` () =
        let expr = Select.Star<Person>().Star<Car>()
                            .From<Person>()
                            .InnerJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
                            .Where(<@ fun (p : Person) -> p.Age > 42 @>)
                            .And(<@ fun (p : Person) (c : Car) -> p.Age > c.Age @>)
                            .Or(<@ fun (p : Person) (c : Car) -> p.Name = c.Brand @>)
        let result = expr.ToSqlExpression()
        result |> should equal conditionalReferencingSeveralTablesWithAndAndOrsExpression
