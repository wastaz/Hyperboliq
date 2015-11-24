namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SubExpressionTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq.Tests.TokenGeneration.TestHelpers
    open Hyperboliq
    open Hyperboliq.Domain
    open Hyperboliq.Domain.AST

    [<Test>]
    let ``It should be possible to compare against a subexpression in a where expression`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .Where(<@ fun (p : Person) -> p.Age > Sql.SubExpr(Select.Column(<@ fun (c : Car) -> c.Age @>)
                                                                                 .From<Car>()
                                                                                 .Where(<@ fun (c : Car) -> c.Id = 42 @>)) @>)
        let result = expr.ToSqlExpression()

        let pref = TableIdentifier<Person>()
        let cref = TableIdentifier<Car>()
        let expected =
            { TestHelpers.EmptySelect with
                Select = { IsDistinct = false; Values = [ ValueNode.StarColumn(pref.Reference) ] }
                From = { Tables = [ pref ]; Joins = [] }
                Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, pref.Reference :> ITableReference)
                                    Operation = BinaryOperation.GreaterThan
                                    Rhs = { TestHelpers.EmptySelect with
                                                Select = { IsDistinct = false; Values = [ ValueNode.Column("Age", typeof<int>, cref.Reference :> ITableReference) ] }
                                                From = { Tables = [ cref ]; Joins = [] }
                                                Where = { Start = { Lhs = ValueNode.Column("Id", typeof<int>, cref.Reference :> ITableReference)
                                                                    Operation = BinaryOperation.Equal
                                                                    Rhs = ValueNode.Constant("42") } |> ValueNode.BinaryExpression
                                                          AdditionalClauses = [] } |> Some
                                          } |> PlainSelectExpression.Plain |> ValueNode.SubExpression
                                  } |> ValueNode.BinaryExpression
                          AdditionalClauses = [] } |> Some 
            } |> TestHelpers.ToPlainSelect
        result |> should equal expected

    [<Test>]
    let ``It should be possible to do an in query with a subexpression`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .Where(<@ fun (p : Person) -> Sql.In(p.Id, Select.Column(<@ fun (c : Car) -> c.DriverId @>).From<Car>()) @>)
        let result = expr.ToSqlExpression()

        let pref = TableIdentifier<Person>()
        let cref = TableIdentifier<Car>()
        let expected = 
            { TestHelpers.EmptySelect with
                Select = { IsDistinct = false; Values = [ ValueNode.StarColumn(pref.Reference) ] }
                From = { Tables = [ pref ]; Joins = [] }
                Where = { Start = { Lhs = ValueNode.Column("Id", typeof<int>, pref.Reference :> ITableReference)
                                    Operation = BinaryOperation.In
                                    Rhs = { TestHelpers.EmptySelect with
                                                Select = { IsDistinct = false
                                                           Values = [ ValueNode.Column("DriverId", typeof<int>, cref.Reference :> ITableReference) ] }
                                                From = { Tables = [ cref ]; Joins = [] } 
                                          } |> PlainSelectExpression.Plain |> ValueNode.SubExpression
                                  } |> ValueNode.BinaryExpression
                          AdditionalClauses = []
                        } |> Some
            } |> TestHelpers.ToPlainSelect
        result |> should equal expected