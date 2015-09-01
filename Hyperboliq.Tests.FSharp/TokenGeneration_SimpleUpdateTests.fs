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

    [<Test>]
    let ``It should be possible to update columns in place`` () =
        let expr = Update<Person>.Set(<@ fun (p : Person) -> p.Age @>, <@ fun (p : Person) -> p.Age + 1 @>)
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected = 
            { UpdateSet = { Table = tref.Reference :> ITableReference 
                            SetExpressions = [ { Column = "Age", typeof<int>, tref.Reference :> ITableReference
                                                 Value = { Operation = BinaryOperation.Add
                                                           Lhs = ValueNode.Column("Age", typeof<int>, tref.Reference :> ITableReference)
                                                           Rhs = ValueNode.Constant("1") } |> ValueNode.BinaryExpression } ] }
              Where = None 
            } |> SqlExpression.Update
        result |> should equal expected

    [<Test>]
    let ``It should be possible to update multiple columns in place`` () =
        let expr = Update<Person>.Set(<@ fun (p : Person) -> (p.Name, p.Age) @>, <@ fun (p : Person) -> ("Kalle" + p.Name, p.Age - 2 ) @>)
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected =
            { UpdateSet = { Table = tref.Reference :> ITableReference 
                            SetExpressions = [ { Column = "Name", typeof<string>, tref.Reference :> ITableReference
                                                 Value = { Operation = BinaryOperation.Add
                                                           Lhs = ValueNode.Constant("'Kalle'")
                                                           Rhs = ValueNode.Column("Name", typeof<string>, tref.Reference :> ITableReference) } |> ValueNode.BinaryExpression }
                                               { Column = "Age", typeof<int>, tref.Reference :> ITableReference
                                                 Value = { Operation = BinaryOperation.Subtract
                                                           Lhs = ValueNode.Column("Age", typeof<int>, tref.Reference :> ITableReference)
                                                           Rhs = ValueNode.Constant("2") } |> ValueNode.BinaryExpression } ] } 
              Where = None
            } |> SqlExpression.Update
        result |> should equal expected

    [<Test>]
    let ``It should be possible to update values to a subexpression`` () =
        let expr = Update<Person>.Set(<@ fun (p : Person) -> p.Age @>,
                                      Select.Column(<@ fun (c : Car) -> Sql.Max(c.Age) @>).From<Car>() :> ISelectExpressionTransformable)
        let result = expr.ToSqlExpression()
        
        let pref = TableIdentifier<Person>()
        let cref = TableIdentifier<Car>()
        let expected =
            { UpdateSet = { Table = pref.Reference :> ITableReference 
                            SetExpressions = [ { Column = "Age", typeof<int>, pref.Reference :> ITableReference
                                                 Value = { TestHelpers.EmptySelect with 
                                                            Select = { IsDistinct = false
                                                                       Values = [ ValueNode.Aggregate(AggregateType.Max, ValueNode.Column("Age", typeof<int>, cref.Reference :> ITableReference)) ] }
                                                            From = { Tables = [ cref ]; Joins = [] } } |> PlainSelectExpression.Plain |> ValueNode.SubExpression } ] }
              Where = None 
            } |> SqlExpression.Update
        result |> should equal expected