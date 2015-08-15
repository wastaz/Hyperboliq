namespace Hyperboliq.Tests.TokenGeneration

module TestModel =

    [<CLIMutable>]
    type Person =
        { Id : int
          Name : string
          Age : int
          LivesAtHouseId : int
          ParentId : int }

    [<CLIMutable>]
    type House =
        { Id : int
          Address : string }
    
    [<CLIMutable>]    
    type Car =
        { Id : int
          Brand : string
          DriverId : int
          Age : int }

module TestHelpers =
    open Hyperboliq.Domain.AST

    let EmptySelect =
        { Select = { IsDistinct = false; Values = [] }
          From = { Tables = []; Joins = [] }
          Where = None
          GroupBy = None
          OrderBy = None }

module SimpleSelectTests =
    open NUnit.Framework
    open FsUnit
    open TestModel
    open Hyperboliq
    open Hyperboliq.Domain.AST
    open Hyperboliq.Domain

    [<Test>]
    let ``It should be able to select all from a table`` () =
        let expr = Select.Star<Person>().From<Person>()
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected =
            { TestHelpers.EmptySelect with
                Select = { IsDistinct = false; Values = [ StarColumn(StarColumnToken(tref.Reference)) ] }
                From = { Tables = [ tref ]; Joins = [] } }
            |> PlainSelectExpression.Plain
            |> SelectExpression.Plain
            |> SqlExpression.Select

        result |> should equal expected

