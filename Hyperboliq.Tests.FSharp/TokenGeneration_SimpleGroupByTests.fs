namespace Hyperboliq.Tests.TokenGeneration

module SimpleGroupByTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq
    open Hyperboliq.Domain.AST
    open Hyperboliq.Domain
    open Hyperboliq.Tests.TokenGeneration.TokenGeneration_SimpleGroupByTests_Results

    [<Test>]
    let ``It should be possible to group by a single column`` () =
        let expr = Select.Column(<@ fun (p : Person) -> let maxAge = Sql.Max(p.Age) in (p.Name, maxAge) @>)
                         .From<Person>()
                         .GroupBy(<@ fun (p : Person) -> p.Name @>)
        let result = expr.ToSqlExpression()
        result |> should equal groupBySingleColumnExpression

    [<Test>]
    let ``It should be possible to group by multiple columns`` () =
        let expr = Select.Column(<@ fun (p : Person) -> let minAge = Sql.Min(p.Age) in (p.Name, p.LivesAtHouseId, minAge) @>)
                         .From<Person>()
                         .GroupBy(<@ fun (p : Person) -> (p.Name, p.LivesAtHouseId) @>)
        let result = expr.ToSqlExpression()
        result |> should equal groupByMultipleColumnsExpression

    [<Test>]
    let ``It should be possible to group by columns from multiple tables`` () =
        let expr = Select.Column(<@ fun (p : Person) -> let averageAge = Sql.Avg(p.Age) in (p.Name, averageAge) @>)
                         .Column(<@ fun (c : Car) -> let minAge = Sql.Min(c.Age) in (c.Brand, minAge) @>)
                         .From<Person>()
                         .InnerJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
                         .GroupBy(<@ fun (p : Person) -> p.Name @>)
                         .ThenBy(<@ fun (c : Car) -> c.Brand @>)
        let result = expr.ToSqlExpression()
        result |> should equal groupByColumnsFromMultipleTablesExpression

    [<Test>]
    let ``It should be possible to use a single having expression`` () =
        let expr = Select.Column(<@ fun (p : Person) -> let averageAge = Sql.Avg(p.Age) in (p.Name, averageAge) @>)
                         .From<Person>()
                         .GroupBy(<@ fun (p : Person) -> p.Name @>)
                         .Having(<@ fun (p : Person) -> Sql.Avg(p.Age) > 42 @>)
        let result = expr.ToSqlExpression()
        result |> should equal groupByWithSingleHavingExpression

    [<Test>]
    let ``It should be possible to use multiple having expressions`` () =
        let expr = Select.Column(<@ fun (p : Person) -> let averageAge = Sql.Avg(p.Age) in (p.Name, averageAge) @>)
                         .Column(<@ fun (c : Car) -> let minAge = Sql.Min(c.Age) in (c.Brand, minAge) @>)
                         .From<Person>()
                         .InnerJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
                         .GroupBy(<@ fun (p : Person) -> p.Name @>)
                         .ThenBy(<@ fun (c : Car) -> c.Brand @>)
                         .Having(<@ fun (p : Person) -> Sql.Avg(p.Age) > 42 @>)
                         .And(<@ fun (c : Car) -> Sql.Min(c.Age) > 2 @>)
        let result = expr.ToSqlExpression()
        result |> should equal groupByWithMultipleHavingExpression
