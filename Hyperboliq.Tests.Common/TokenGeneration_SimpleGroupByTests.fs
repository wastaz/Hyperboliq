namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SimpleGroupByTests_Results =
  open Hyperboliq
  open Hyperboliq.Domain.AST
  open Hyperboliq.Domain

  let personRef = TableIdentifier<Person>()
  let pref = personRef.Reference :> ITableReference

  let carRef = TableIdentifier<Car>()
  let cref = carRef.Reference :> ITableReference

  let groupBySingleColumnExpression =
    { TestHelpers.EmptySelect with 
        Select = { IsDistinct = false
                   Values = [ ValueNode.Column("Name", typeof<string>, pref)
                              ValueNode.NamedColumn({ Alias = "maxAge"
                                                      Column = ValueNode.Aggregate(AggregateType.Max, ValueNode.Column("Age", typeof<int>, pref)) }) ] } 
        From = { Tables = [ personRef ]; Joins = [] }
        GroupBy = { Clauses = [ ValueNode.Column("Name", typeof<string>, pref) ]
                    Having = [] } |> Some
    } |> TestHelpers.ToPlainSelect

  let groupByMultipleColumnsExpression =
    { TestHelpers.EmptySelect with
        Select =  { IsDistinct = false
                    Values = [ ValueNode.Column("Name", typeof<string>, pref)
                               ValueNode.Column("LivesAtHouseId", typeof<int>, pref)
                               ValueNode.NamedColumn({ Alias = "minAge"
                                                       Column = ValueNode.Aggregate(AggregateType.Min, ValueNode.Column("Age", typeof<int>, pref)) }) ] } 
        From = { Tables = [ personRef ]; Joins = [] }
        GroupBy = { Clauses = [ ValueNode.Column("Name", typeof<string>, pref)
                                ValueNode.Column("LivesAtHouseId", typeof<int>, pref) ]
                    Having = [] } |> Some
    } |> TestHelpers.ToPlainSelect

  let groupByColumnsFromMultipleTablesExpression =
    { TestHelpers.EmptySelect with
        Select =  { IsDistinct = false
                    Values = [ ValueNode.Column("Brand", typeof<string>, cref)
                               ValueNode.NamedColumn({ Alias = "minAge"
                                                       Column = ValueNode.Aggregate(AggregateType.Min, ValueNode.Column("Age", typeof<int>, cref)) })
                               ValueNode.Column("Name", typeof<string>, pref)
                               ValueNode.NamedColumn({ Alias = "averageAge"
                                                       Column = ValueNode.Aggregate(AggregateType.Avg, ValueNode.Column("Age", typeof<int>, pref)) }) ] } 
        From = { Tables = [ personRef ]
                 Joins = [ { SourceTables = [ personRef ]
                             TargetTable = carRef
                             Type = JoinType.InnerJoin
                             Condition = { Operation = BinaryOperation.Equal
                                           Lhs = ValueNode.Column("Id", typeof<int>, pref)
                                           Rhs = ValueNode.Column("DriverId", typeof<int>, cref) } |> ValueNode.BinaryExpression |> Some } ] }
        GroupBy = { Clauses = [ ValueNode.Column("Name", typeof<string>, pref)
                                ValueNode.Column("Brand", typeof<string>, cref) ] 
                    Having = [] } |> Some
    } |> TestHelpers.ToPlainSelect

  let groupByWithSingleHavingExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.Column("Name", typeof<string>, pref)
                              ValueNode.NamedColumn({ Alias = "averageAge"
                                                      Column = ValueNode.Aggregate(AggregateType.Avg, ValueNode.Column("Age", typeof<int>, pref)) }) ] } 
        From = { Tables = [ personRef ]; Joins = [] }
        GroupBy = { Clauses = [ ValueNode.Column("Name", typeof<string>, pref) ]
                    Having = [ { Combinator = ExpressionCombinatorType.And
                                 Expression = { Operation = BinaryOperation.GreaterThan
                                                Lhs = ValueNode.Aggregate(AggregateType.Avg, ValueNode.Column("Age", typeof<int>, pref))
                                                Rhs = ValueNode.Constant("42") } |> ValueNode.BinaryExpression }  ] } |> Some
    } |> TestHelpers.ToPlainSelect

  let groupByWithMultipleHavingExpression =
    { TestHelpers.EmptySelect with 
        Select = { IsDistinct = false
                   Values = [ ValueNode.Column("Brand", typeof<string>, cref)
                              ValueNode.NamedColumn({ Alias = "minAge"
                                                      Column = ValueNode.Aggregate(AggregateType.Min, ValueNode.Column("Age", typeof<int>, cref)) })
                              ValueNode.Column("Name", typeof<string>, pref)
                              ValueNode.NamedColumn({ Alias = "averageAge"
                                                      Column = ValueNode.Aggregate(AggregateType.Avg, ValueNode.Column("Age", typeof<int>, pref)) }) ] }
        From = { Tables = [ personRef ]
                 Joins = [ { SourceTables = [ personRef ]
                             TargetTable = carRef
                             Type = JoinType.InnerJoin
                             Condition = { Operation = BinaryOperation.Equal
                                           Lhs = ValueNode.Column("Id", typeof<int>, pref)
                                           Rhs = ValueNode.Column("DriverId", typeof<int>, cref) } |> ValueNode.BinaryExpression |> Some } ] }
        GroupBy = { Clauses = [ ValueNode.Column("Name", typeof<string>, pref)
                                ValueNode.Column("Brand", typeof<string>, cref) ]
                    Having = [ { Combinator = ExpressionCombinatorType.And 
                                 Expression = { Operation = BinaryOperation.GreaterThan
                                                Lhs = ValueNode.Aggregate(AggregateType.Min, ValueNode.Column("Age", typeof<int>, cref))
                                                Rhs = ValueNode.Constant("2") } |> ValueNode.BinaryExpression }
                               { Combinator = ExpressionCombinatorType.And
                                 Expression = { Operation = BinaryOperation.GreaterThan
                                                Lhs = ValueNode.Aggregate(AggregateType.Avg, ValueNode.Column("Age", typeof<int>, pref))
                                                Rhs = ValueNode.Constant("42") } |> ValueNode.BinaryExpression } ] } |> Some
    } |> TestHelpers.ToPlainSelect
