namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_LimitOffsetTests_Result =
  open Hyperboliq
  open Hyperboliq.Domain.AST
  open Hyperboliq.Domain
  
  let animalRef = TableIdentifier<Animal>()
  let aref = animalRef.Reference :> ITableReference
  
  let private baseExpr = 
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false; Values = [ValueNode.StarColumn(aref)]}
        From = { Tables = [ animalRef ]; Joins = [] }
        OrderBy = { OrderByExpressionNode.Clauses = 
                    [ { Direction = Direction.Ascending
                        NullsOrdering = NullsOrdering.NullsUndefined
                        Selector = ValueNode.Column("Name", typeof<string>, aref) } ] } |> Some
        LimitOffset = { Limit = Some 10; Offset = Some 15 }
    }
        
  let limitOffsetExpression =
    baseExpr |> TestHelpers.ToPlainSelect

  let limitExpression =
    { baseExpr with LimitOffset = { baseExpr.LimitOffset with Offset = None } } |> TestHelpers.ToPlainSelect
    
  let offsetExpression =
    { baseExpr with LimitOffset = { baseExpr.LimitOffset with Limit = None } } |> TestHelpers.ToPlainSelect

  let noLimitOrOffsetExpression =
    { baseExpr with LimitOffset = { baseExpr.LimitOffset with Offset = None; Limit = None } } |> TestHelpers.ToPlainSelect