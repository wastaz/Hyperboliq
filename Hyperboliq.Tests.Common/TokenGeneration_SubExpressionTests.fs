namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SubExpression_Results =
  open Hyperboliq
  open Hyperboliq.Domain.AST
  open Hyperboliq.Domain

  let personRef = TableIdentifier<Person>()
  let pref = personRef.Reference :> ITableReference

  let carRef = TableIdentifier<Car>()
  let cref = carRef.Reference :> ITableReference

  let compareAgainstSubExprInWhereExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false; Values = [ ValueNode.StarColumn(pref) ] }
        From = { Tables = [ personRef ]; Joins = [] }
        Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, pref)
                            Operation = BinaryOperation.GreaterThan
                            Rhs = { TestHelpers.EmptySelect with
                                        Select = { IsDistinct = false; Values = [ ValueNode.Column("Age", typeof<int>, cref) ] }
                                        From = { Tables = [ carRef ]; Joins = [] }
                                        Where = { Start = { Lhs = ValueNode.Column("Id", typeof<int>, cref)
                                                            Operation = BinaryOperation.Equal
                                                            Rhs = ValueNode.Constant("42") } |> ValueNode.BinaryExpression
                                                  AdditionalClauses = [] } |> Some
                                  } |> PlainSelectExpression.Plain |> ValueNode.SubExpression
                          } |> ValueNode.BinaryExpression
                  AdditionalClauses = [] } |> Some 
    } |> TestHelpers.ToPlainSelect

  let subExprInInExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false; Values = [ ValueNode.StarColumn(pref) ] }
        From = { Tables = [ personRef ]; Joins = [] }
        Where = { Start = { Lhs = ValueNode.Column("Id", typeof<int>, pref)
                            Operation = BinaryOperation.In
                            Rhs = { TestHelpers.EmptySelect with
                                        Select = { IsDistinct = false
                                                   Values = [ ValueNode.Column("DriverId", typeof<int>, cref) ] }
                                        From = { Tables = [ carRef ]; Joins = [] } 
                                  } |> PlainSelectExpression.Plain |> ValueNode.SubExpression
                          } |> ValueNode.BinaryExpression
                  AdditionalClauses = []
                } |> Some
    } |> TestHelpers.ToPlainSelect
