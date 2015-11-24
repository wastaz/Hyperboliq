namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SelectPartitionTests_Results =
  open Hyperboliq
  open Hyperboliq.Domain.AST
  open Hyperboliq.Domain

  let personRef = TableIdentifier<Person>()
  let pref = personRef.Reference :> ITableReference

  let emptyOverClauseExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.Column("Name", typeof<string>, pref)
                              ValueNode.WindowedColumn((AggregateType.Sum, ValueNode.Column("Age", typeof<int>, pref)), 
                                                       { PartitionBy = []; OrderBy = [] }) ] }
        From = { Tables = [ personRef ]; Joins = [] }
    } |> TestHelpers.ToPlainSelect

  let partitionByColumnExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.Column("Name", typeof<string>, pref) 
                              ValueNode.WindowedColumn((AggregateType.Max, ValueNode.Column("Age", typeof<int>, pref)),
                                                       { PartitionBy = [ ValueNode.Column("Name", typeof<string>, pref) ]; OrderBy = [] }) ] }
        From = { Tables = [ personRef ]; Joins = [] }
    } |> TestHelpers.ToPlainSelect

  let partitionByMultipleColumnsExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.Column("Name", typeof<string>, pref)
                              ValueNode.WindowedColumn((AggregateType.Max, ValueNode.Column("Age", typeof<int>, pref)),
                                                       { PartitionBy = [ ValueNode.Column("Name", typeof<string>, pref)
                                                                         ValueNode.Column("LivesAtHouseId", typeof<int>, pref) ]
                                                         OrderBy = [] })
                               ] }
        From = { Tables = [ personRef ]; Joins = [] }
    } |> TestHelpers.ToPlainSelect

  let orderByColumnExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.Column("Name", typeof<string>, pref)
                              ValueNode.WindowedColumn((AggregateType.Sum, ValueNode.Column("Age", typeof<int>, pref)),
                                                       { PartitionBy = []
                                                         OrderBy = [ { Direction = Direction.Ascending
                                                                       NullsOrdering = NullsOrdering.NullsUndefined
                                                                       Selector = ValueNode.Column("Age", typeof<int>, pref) } ] 
                                                       }) ] }
        From = { Tables = [ personRef ]; Joins = [] }
    } |> TestHelpers.ToPlainSelect

  let orderByMultipleColumnsExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.Column("Name", typeof<string>, pref)
                              ValueNode.WindowedColumn((AggregateType.Sum, ValueNode.Column("Age", typeof<int>, pref)),
                                                       { PartitionBy = []
                                                         OrderBy = [ { Direction = Direction.Ascending
                                                                       NullsOrdering = NullsOrdering.NullsLast
                                                                       Selector = ValueNode.Column("Age", typeof<int>, pref) }
                                                                     { Direction = Direction.Descending
                                                                       NullsOrdering = NullsOrdering.NullsUndefined
                                                                       Selector = ValueNode.Column("Name", typeof<string>, pref) } ] 
                                                       }) ] }
        From = { Tables = [ personRef ]; Joins = [] }
    } |> TestHelpers.ToPlainSelect

  let orderAndPartitionByColumnExpression =
    { TestHelpers.EmptySelect with
        Select = 
          { IsDistinct = false
            Values = [ ValueNode.Column("Name", typeof<string>, pref)
                       ValueNode.WindowedColumn((AggregateType.Sum, ValueNode.Column("Age", typeof<int>, pref)),
                                                { PartitionBy = [ ValueNode.Column("Name", typeof<string>, pref) ]
                                                  OrderBy = [ { Direction = Direction.Ascending
                                                                NullsOrdering = NullsOrdering.NullsUndefined
                                                                Selector = ValueNode.Column("Age", typeof<int>, pref) } ] 
                                                }) ] }
        From = { Tables = [ personRef ]; Joins = [] }
    } |> TestHelpers.ToPlainSelect

