namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SimpleUpdateTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq.Tests.TokenGeneration.TestModel
    open Hyperboliq.Tests.TokenGeneration.TestHelpers
    open Hyperboliq.Domain
    open Hyperboliq.Domain.AST
    open Hyperboliq

    [<Test>]
    let ``It should be possible to perform a global update`` () =
        let expr = Update<Person>.Set(<@ fun (p : Person) -> p.Name @>, "Kalle")
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected = 
            { UpdateSet = { Table = tref.Reference :> ITableReference
                            SetExpressions = [ { Column = "Name", typeof<string>, tref.Reference :> ITableReference
                                                 Value = ValueNode.Constant("'Kalle'") } ] }
              Where = None 
            } |> SqlExpression.Update
        result |> should equal expected

    [<Test>]
    let ``It should be possible to set multiple values in a single statement`` () = 
        let expr = Update<Person>.Set(<@ fun (p : Person) -> (p.Name, p.Age) @>, 
                                      ("Kalle", 42))
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected = 
            { UpdateSet = { Table = tref.Reference :> ITableReference
                            SetExpressions =  [ { Column = "Name", typeof<string>, tref.Reference :> ITableReference
                                                  Value = ValueNode.Constant("'Kalle'") }
                                                { Column = "Age", typeof<int>, tref.Reference :> ITableReference
                                                  Value = ValueNode.Constant("42") } ] }
              Where = None 
            } |> SqlExpression.Update
        result |> should equal expected
