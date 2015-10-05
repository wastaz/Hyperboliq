namespace Hyperboliq.Tests.TokenGeneration

module SimpleDeleteTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq
    open Hyperboliq.Domain.AST
    open Hyperboliq.Domain

    [<Test>]
    let ``It should be possible to generate a delete all rows from table statement`` () =
        let expr = Delete.From<Person>()
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected = 
            { From = { Tables = [ tref ]; Joins = [] }
              Where = None 
            } |> SqlExpression.Delete
        
        result |> should equal expected

    [<Test>]
    let ``It should be possible to put a where expression on a delete statement`` () =
        let expr = Delete.From<Person>()
                         .Where(<@ fun (p : Person) -> p.Age > 42 @>)
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected =
            { From = { Tables = [ tref ]; Joins = [] }
              Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, tref.Reference :> ITableReference)
                                  Operation = BinaryOperation.GreaterThan
                                  Rhs = ValueNode.Constant("42") } |> ValueNode.BinaryExpression
                        AdditionalClauses = [] } |> Some 
            } |> SqlExpression.Delete
        result |> should equal expected

    [<Test>]
    let ``It should be possible to use and and or on a where statement in a delete statement`` () =
        let expr = Delete.From<Person>()
                         .Where(<@ fun (p : Person) -> p.Age > 42@>)
                         .And(<@ fun (p : Person) -> p.Name = "Kalle" @>)
                         .Or(<@ fun (p : Person) -> p.Name = "Henrik" @>)
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected =
            { From = { Tables = [ tref ]; Joins = [] }
              Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, tref.Reference :> ITableReference)
                                  Operation = BinaryOperation.GreaterThan
                                  Rhs = ValueNode.Constant("42") } |> ValueNode.BinaryExpression 
                        AdditionalClauses = 
                            [ { Combinator = ExpressionCombinatorType.Or
                                Expression = { Lhs = ValueNode.Column("Name", typeof<string>, tref.Reference :> ITableReference)
                                               Operation = BinaryOperation.Equal
                                               Rhs = ValueNode.Constant("'Henrik'") } |> ValueNode.BinaryExpression }
                              { Combinator = ExpressionCombinatorType.And 
                                Expression = { Lhs = ValueNode.Column("Name", typeof<string>, tref.Reference :> ITableReference)
                                               Operation = BinaryOperation.Equal
                                               Rhs = ValueNode.Constant("'Kalle'") } |> ValueNode.BinaryExpression }
                            ] } |> Some
            } |> SqlExpression.Delete
        result |> should equal expected

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

        let pref = TableIdentifier<Person>()
        let cref = TableIdentifier<Car>()
        let expected =
            { From = { Tables = [ cref ]; Joins = [] }
              Where = { Start = { Lhs = ValueNode.Column("DriverId", typeof<int>, cref.Reference :> ITableReference)
                                  Operation = BinaryOperation.In
                                  Rhs = ValueNode.SubExpression(
                                            { TestHelpers.EmptySelect with 
                                                Select = { IsDistinct = false
                                                           Values = [ ValueNode.Column("Id", typeof<int>, pref.Reference :> ITableReference) ] }
                                                From = { Tables = [ pref ]; Joins = [] }
                                                Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, pref.Reference :> ITableReference)
                                                                    Operation = BinaryOperation.GreaterThan
                                                                    Rhs = ValueNode.Constant("18") } |> ValueNode.BinaryExpression
                                                          AdditionalClauses = []
                                                        } |> Some
                                            } |> PlainSelectExpression.Plain) 
                                } |> ValueNode.BinaryExpression
                        AdditionalClauses = [] } |> Some 
            } |> SqlExpression.Delete
        result |> should equal expected