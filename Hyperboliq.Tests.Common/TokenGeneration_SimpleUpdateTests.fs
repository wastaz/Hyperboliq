namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SimpleUpdateTests_Results =
  open Hyperboliq
  open Hyperboliq.Domain.AST
  open Hyperboliq.Domain

  let personRef = TableIdentifier<Person>()
  let pref = personRef.Reference :> ITableReference

  let carRef = TableIdentifier<Car>()
  let cref = carRef.Reference :> ITableReference

  let globalUpdateExpression =
    { UpdateSet = { Table = pref
                    SetExpressions = [ { Column = "Name", typeof<string>, pref
                                         Value = ValueNode.Constant("'Kalle'") } ] }
      Where = None 
    } |> SqlExpression.Update

  let multipleSetsExpression =
    { UpdateSet = { Table = pref
                    SetExpressions =  [ { Column = "Name", typeof<string>, pref
                                          Value = ValueNode.Constant("'Kalle'") }
                                        { Column = "Age", typeof<int>, pref
                                          Value = ValueNode.Constant("42") } ] }
      Where = None 
    } |> SqlExpression.Update

  let updateInPlaceExpression =
    { UpdateSet = { Table = pref
                    SetExpressions = [ { Column = "Age", typeof<int>, pref
                                         Value = { Operation = BinaryOperation.Add
                                                   Lhs = ValueNode.Column("Age", typeof<int>, pref)
                                                   Rhs = ValueNode.Constant("1") } |> ValueNode.BinaryExpression } ] }
      Where = None 
    } |> SqlExpression.Update

  let updateMultipleColumnsInPlaceExpression =
    { UpdateSet = { Table = pref
                    SetExpressions = [ { Column = "Name", typeof<string>, pref
                                         Value = { Operation = BinaryOperation.Add
                                                   Lhs = ValueNode.Constant("'Kalle'")
                                                   Rhs = ValueNode.Column("Name", typeof<string>, pref) } |> ValueNode.BinaryExpression }
                                       { Column = "Age", typeof<int>, pref
                                         Value = { Operation = BinaryOperation.Subtract
                                                   Lhs = ValueNode.Column("Age", typeof<int>, pref)
                                                   Rhs = ValueNode.Constant("2") } |> ValueNode.BinaryExpression } ] } 
      Where = None
    } |> SqlExpression.Update

  let updateValuesToSubExpression =
    { UpdateSet = 
        { Table = pref
          SetExpressions = 
            [ { Column = "Age", typeof<int>, pref
                Value = { TestHelpers.EmptySelect with 
                            Select = { IsDistinct = false
                                       Values = [ ValueNode.Aggregate(AggregateType.Max, ValueNode.Column("Age", typeof<int>, cref)) ] }
                            From = { Tables = [ carRef ]; Joins = [] } } |> PlainSelectExpression.Plain |> ValueNode.SubExpression } ] }
      Where = None 
    } |> SqlExpression.Update

  let conditionalUpdateExpression =
    { UpdateSet = { Table = pref
                    SetExpressions = [ { Column = "Age", typeof<int>, pref
                                         Value = ValueNode.Constant("42") } ] }
      Where = { Start = { Operation = BinaryOperation.Equal
                          Lhs = ValueNode.Column("Name", typeof<string>, pref)
                          Rhs = ValueNode.Constant("'Kalle'") } |> ValueNode.BinaryExpression
                AdditionalClauses = [] } |> Some 
    } |> SqlExpression.Update

  let multipleConditionsUpdateExpression =
    { UpdateSet = { Table = pref
                    SetExpressions = [ { Column = "Age", typeof<int>, pref
                                         Value = ValueNode.Constant("42") } ] }
      Where = { Start = { Operation = BinaryOperation.Equal
                          Lhs = ValueNode.Column("Name", typeof<string>, pref) 
                          Rhs = ValueNode.Constant("'Kalle'") } |> ValueNode.BinaryExpression 
                AdditionalClauses = [ { Combinator = ExpressionCombinatorType.And 
                                        Expression = 
                                            { Operation = BinaryOperation.LessThan
                                              Lhs = ValueNode.Column("Age", typeof<int>, pref) 
                                              Rhs = ValueNode.Constant("18") } |> ValueNode.BinaryExpression }
                                      { Combinator = ExpressionCombinatorType.Or
                                        Expression =
                                            { Operation = BinaryOperation.Equal
                                              Lhs = ValueNode.Column("Name", typeof<string>, pref)
                                              Rhs = ValueNode.Constant("'Pelle'") } |> ValueNode.BinaryExpression }
                                    ] } |> Some 
    } |> SqlExpression.Update
    