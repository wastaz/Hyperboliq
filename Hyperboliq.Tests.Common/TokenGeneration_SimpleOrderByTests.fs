namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SimpleOrderByTests_Results =
  open Hyperboliq
  open Hyperboliq.Domain.AST
  open Hyperboliq.Domain

  let personRef = TableIdentifier<Person>()
  let pref = personRef.Reference :> ITableReference

  let carRef = TableIdentifier<Car>()
  let cref = carRef.Reference :> ITableReference

  let GetOrderByDirectionExpression direction =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.StarColumn(pref) ] }
        From = { Tables = [ personRef ]; Joins = [] }
        OrderBy = { OrderByExpressionNode.Clauses = 
                        [ { Direction = direction
                            NullsOrdering = NullsOrdering.NullsUndefined
                            Selector = ValueNode.Column("Age", typeof<int>, pref) } ] } |> Some
    } |> TestHelpers.ToPlainSelect

  let orderByMultipleColumnsExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.StarColumn(cref); ValueNode.StarColumn(pref) ] }
        From = { Tables = [ personRef ]
                 Joins = [ { SourceTables = [ personRef ]
                             TargetTable = carRef
                             Type = JoinType.InnerJoin
                             Condition = { Operation = BinaryOperation.Equal
                                           Lhs = ValueNode.Column("Id", typeof<int>, pref)
                                           Rhs = ValueNode.Column("DriverId", typeof<int>, cref) } |> ValueNode.BinaryExpression |> Some } ] }
        OrderBy = { OrderByExpressionNode.Clauses = 
                        [ { Direction = Descending
                            NullsOrdering = NullsOrdering.NullsUndefined
                            Selector = ValueNode.Column("Brand", typeof<string>, cref) }
                          { Direction = Ascending
                            NullsOrdering = NullsOrdering.NullsUndefined
                            Selector = ValueNode.Column("Age", typeof<int>, pref) } ] } |> Some
    } |> TestHelpers.ToPlainSelect

  let GetOrderByNullsOrderingExpression nullsOrdering =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.StarColumn(pref) ] } 
        From = { Tables = [ personRef ]; Joins = [] }
        OrderBy = { OrderByExpressionNode.Clauses = 
                        [ { Direction = Direction.Ascending
                            NullsOrdering = nullsOrdering
                            Selector = ValueNode.Column("Age", typeof<int>, pref) } ] } |> Some
    } |> TestHelpers.ToPlainSelect

  let orderByWithDifferentNullsOrderingPerColumnExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.StarColumn(pref) ] }
        From = { Tables = [ personRef ]; Joins = [] }
        OrderBy = { OrderByExpressionNode.Clauses =
                        [ { Direction = Direction.Descending
                            NullsOrdering = NullsOrdering.NullsFirst
                            Selector = ValueNode.Column("Name", typeof<string>, pref) }
                          { Direction = Direction.Ascending
                            NullsOrdering = NullsOrdering.NullsLast
                            Selector = ValueNode.Column("Age", typeof<int>, pref) } ] } |> Some
    } |> TestHelpers.ToPlainSelect
