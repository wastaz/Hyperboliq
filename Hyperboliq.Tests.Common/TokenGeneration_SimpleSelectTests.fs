namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SimpleSelectTests_Results =
  open Hyperboliq
  open Hyperboliq.Domain.AST
  open Hyperboliq.Domain

  let personRef = TableIdentifier<Person>()

  let selectAllExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false; Values = [ StarColumn(personRef.Reference) ] }
        From = { Tables = [ personRef ]; Joins = [] } }
    |> TestHelpers.ToPlainSelect

  let selectDistinctExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = true; Values = [ StarColumn(personRef.Reference) ] }
        From = { Tables = [ personRef ]; Joins = [] } }
    |> TestHelpers.ToPlainSelect

  let selectConstantExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.NamedColumn({ Alias = "favoriteNumber"; Column = ValueNode.Constant("42") })
                              ValueNode.NamedColumn({ Alias = "name"; Column = ValueNode.Column("Name", typeof<string>, personRef.Reference :> ITableReference)}) ] } 
        From = { Tables = [ personRef ]; Joins = [] } }
    |> TestHelpers.ToPlainSelect

  let selectColumnsExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.Column("Name", typeof<string>, personRef.Reference :> ITableReference)
                              ValueNode.Column("Age", typeof<int>, personRef.Reference :> ITableReference) ] }
        From = { Tables = [ personRef ]; Joins = [] } }
    |> TestHelpers.ToPlainSelect

  let selectDistinctSingleColumnsExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = true
                   Values = [ ValueNode.Column("Age", typeof<int>, personRef.Reference :> ITableReference) ] }
        From = { Tables = [ personRef ]; Joins = [] } }
    |> TestHelpers.ToPlainSelect

  let selectCountExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.Aggregate(AggregateType.Count, ValueNode.NullValue) ] }
        From = { Tables = [ personRef ]; Joins = [] } }
    |> TestHelpers.ToPlainSelect

  let selectNamedCountExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.NamedColumn({ Alias = "numberOfPersons"; Column = ValueNode.Aggregate(AggregateType.Count, ValueNode.NullValue) })] }
        From = { Tables = [ personRef ]; Joins = [] } }
    |> TestHelpers.ToPlainSelect