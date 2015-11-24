namespace Hyperboliq.Tests.TokenGeneration

module SimpleSelectTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq
    open Hyperboliq.Domain.AST
    open Hyperboliq.Domain
    open Hyperboliq.Tests.TokenGeneration.TokenGeneration_SimpleSelectTests_Results

    [<Test>]
    let ``It should be able to select all from a table`` () =
        let expr = Select.Star<Person>().From<Person>()
        let result = expr.ToSqlExpression()

        result |> should equal selectAllExpression

    [<Test>]
    let ``It should be possible to select distinct from a table`` () =
        let expr = Select.Distinct.Star<Person>().From<Person>()
        let result = expr.ToSqlExpression()
        
        result |> should equal selectDistinctExpression

    [<Test>]
    let ``It should be possible to select a constant`` () =
        let expr = Select.Column(fun (p : Person) -> let (favoriteNumber, name) = (42, p.Name) in (favoriteNumber, name))
                         .From<Person>()
        let result = expr.ToSqlExpression()

        result |> should equal selectConstantExpression

    [<Test>]
    let ``It should be able to select columns`` () =
        let expr = Select.Column(<@ fun (p : Person) -> (p.Name, p.Age) @>)
                         .From<Person>()
        let result = expr.ToSqlExpression()

        result |> should equal selectColumnsExpression

    [<Test>]
    let ``It should order the columns in the expected order when calling column several times`` () =
        let expr = Select.Column(<@ fun (p : Person) -> p.Name @>).Column(<@ fun (p : Person) -> p.Age @>)
                         .From<Person>()
        let result = expr.ToSqlExpression()

        result |> should equal selectColumnsExpression

    [<Test>]
    let ``It should be possible to select distinct single columns from a table`` () =
        let expr = Select.Distinct.Column(<@ fun (p : Person) -> p.Age @>)
                         .From<Person>()
        let result = expr.ToSqlExpression()

        result |> should equal selectDistinctSingleColumnsExpression

    [<Test>]
    let ``It should be possible to select the number of rows from a table`` () = 
        let expr = Select.Column(<@ fun (p : Person) -> Sql.Count() @>).From<Person>()
        let result = expr.ToSqlExpression()

        result |> should equal selectCountExpression

    [<Test>]
    let ``It should be possible to select the number of rows from a table and name the column`` () =
        let expr = Select.Column(<@ fun (p : Person) -> let numberOfPersons = Sql.Count() in numberOfPersons @>)
                         .From<Person>()
        let result = expr.ToSqlExpression()

        result |> should equal selectNamedCountExpression
