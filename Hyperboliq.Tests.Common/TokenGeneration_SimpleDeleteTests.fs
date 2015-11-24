namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SimpleDeleteTests_Results =
  open Hyperboliq
  open Hyperboliq.Domain.AST
  open Hyperboliq.Domain

  let personRef = TableIdentifier<Person>()
  let carRef = TableIdentifier<Car>()

  let deleteAllRowsExpression = 
      { From = { Tables = [ personRef ]; Joins = [] }
        Where = None 
      } |> SqlExpression.Delete
        
  let deleteWithWhereExpression =
      { From = { Tables = [ personRef ]; Joins = [] }
        Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, personRef.Reference :> ITableReference)
                            Operation = BinaryOperation.GreaterThan
                            Rhs = ValueNode.Constant("42") } |> ValueNode.BinaryExpression
                  AdditionalClauses = [] } |> Some 
      } |> SqlExpression.Delete

  let deleteWithWhereWithAndOrExpression =
      { From = { Tables = [ personRef ]; Joins = [] }
        Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, personRef.Reference :> ITableReference)
                            Operation = BinaryOperation.GreaterThan
                            Rhs = ValueNode.Constant("42") } |> ValueNode.BinaryExpression 
                  AdditionalClauses = 
                      [ { Combinator = ExpressionCombinatorType.Or
                          Expression = { Lhs = ValueNode.Column("Name", typeof<string>, personRef.Reference :> ITableReference)
                                         Operation = BinaryOperation.Equal
                                         Rhs = ValueNode.Constant("'Henrik'") } |> ValueNode.BinaryExpression }
                        { Combinator = ExpressionCombinatorType.And 
                          Expression = { Lhs = ValueNode.Column("Name", typeof<string>, personRef.Reference :> ITableReference)
                                         Operation = BinaryOperation.Equal
                                         Rhs = ValueNode.Constant("'Kalle'") } |> ValueNode.BinaryExpression }
                      ] } |> Some
      } |> SqlExpression.Delete

  let deleteWithSubQueryExpression =
    { From = { Tables = [ carRef ]; Joins = [] }
      Where = { Start = { Lhs = ValueNode.Column("DriverId", typeof<int>, carRef.Reference :> ITableReference)
                          Operation = BinaryOperation.In
                          Rhs = ValueNode.SubExpression(
                                    { TestHelpers.EmptySelect with 
                                        Select = { IsDistinct = false
                                                   Values = [ ValueNode.Column("Id", typeof<int>, personRef.Reference :> ITableReference) ] }
                                        From = { Tables = [ personRef ]; Joins = [] }
                                        Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, personRef.Reference :> ITableReference)
                                                            Operation = BinaryOperation.GreaterThan
                                                            Rhs = ValueNode.Constant("18") } |> ValueNode.BinaryExpression
                                                  AdditionalClauses = []
                                                } |> Some
                                    } |> PlainSelectExpression.Plain) 
                        } |> ValueNode.BinaryExpression
                AdditionalClauses = [] } |> Some 
    } |> SqlExpression.Delete