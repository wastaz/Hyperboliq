namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SimpleWhereTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq.Tests.TokenGeneration.TestModel
    open Hyperboliq.Tests.TokenGeneration.TestHelpers
    open Hyperboliq
    open Hyperboliq.Domain
    open Hyperboliq.Domain.AST

    [<Test>]
    let ``It should handle a simple where condition`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .Where(<@ fun (p : Person) -> p.Age > 42 @>)
        let result = expr.ToSqlExpression()
        
        let tref = TableIdentifier<Person>()
        let expected =
            { TestHelpers.EmptySelect with
                Select = { IsDistinct = false
                           Values = [ ValueNode.StarColumn(StarColumnToken(tref.Reference)) ] }
                From = { Tables = [ tref ]; Joins = [] }
                Where = { Start = { Lhs = ValueNode.Column("Age", typeof<Person>, tref.Reference :> ITableReference)
                                    Rhs = ValueNode.Constant(ConstantNode("42"))
                                    Operation = BinaryOperation.GreaterThan } 
                                  |> ValueNode.BinaryExpression
                          AdditionalClauses = [] } |> Some }
            |> TestHelpers.ToPlainSelect
        result |> should equal expected

    [<Test>]
    let ``It should handle a conditional with and and ors in the expression`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .Where(<@ fun (p : Person) -> p.Age > 42 || (p.Age < 10 && p.Name = "Karl") @>)
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected =
            { TestHelpers.EmptySelect with
                Select = { IsDistinct = false
                           Values = [ ValueNode.StarColumn(StarColumnToken(tref.Reference)) ] }
                From = { Tables = [ tref ]; Joins = [] }
                Where = { Start = { Operation = BinaryOperation.Or
                                    Rhs = { Operation = BinaryOperation.And
                                            Lhs = { Operation = BinaryOperation.LessThan
                                                    Lhs = ValueNode.Column("Age", typeof<Person>, tref.Reference :> ITableReference)
                                                    Rhs = ValueNode.Constant(ConstantNode("10"))
                                                  } |> ValueNode.BinaryExpression
                                            Rhs = { Operation = BinaryOperation.Equal
                                                    Lhs = ValueNode.Column("Name", typeof<Person>, tref.Reference :> ITableReference)
                                                    Rhs = ValueNode.Constant(ConstantNode("'Karl'")) 
                                                  } |> ValueNode.BinaryExpression 
                                          } |> ValueNode.BinaryExpression
                                    Lhs = { Operation = BinaryOperation.GreaterThan
                                            Lhs = ValueNode.Column("Age", typeof<Person>, tref.Reference :> ITableReference)
                                            Rhs = ValueNode.Constant(ConstantNode("42")) 
                                          } |> ValueNode.BinaryExpression
                                  } |> ValueNode.BinaryExpression
                          AdditionalClauses = [] } |> Some }
            |> TestHelpers.ToPlainSelect
        result |> should equal expected