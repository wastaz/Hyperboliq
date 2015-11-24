namespace Hyperboliq.Tests.TokenGeneration

module ParameterizedQueryTests = 
    open NUnit.Framework
    open FsUnit
    open Hyperboliq
    open Hyperboliq.Domain.AST
    open Hyperboliq.Domain
    open Hyperboliq.Tests.TokenGeneration.TokenGeneration_ParametrizedQueryTests_Result

    [<Test>]
    let ``It should be able to use a one off parameter in a query`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .Where(<@ fun (p : Person) -> p.Age < Sql.Parameter (ExpressionParameter<int>("age")) @>)
        let result = expr.ToSqlExpression()

        result |> should equal oneOffParameterExpression

    [<Test>]
    let ``It should be able to parameterize a query`` () =
        let ageParam = ExpressionParameter<int>("age")
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .Where(<@ fun (p : Person) -> p.Age > Sql.Parameter ageParam @>)
        let result = expr.ToSqlExpression()

        result |> should equal parameterizedExpression

    [<Test>]
    let ``It should be possible to use the parameter in more complex expression`` () =
        let ageParam = ExpressionParameter<int>("age")
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .Where(<@ fun (p : Person) -> p.Age > Sql.Parameter ageParam && Sql.Parameter ageParam < 90 @>)
                         .Or(<@ fun (p : Person) -> p.Age < Sql.Parameter ageParam @>)
        let result = expr.ToSqlExpression()

        result |> should equal complexParameterizedExpression