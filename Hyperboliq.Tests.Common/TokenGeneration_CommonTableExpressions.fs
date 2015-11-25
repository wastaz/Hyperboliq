namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_CommonTableExpressions_Results =
  open Hyperboliq
  open Hyperboliq.Domain.AST
  open Hyperboliq.Domain

  let personRef = TableIdentifier<Person>()
  let pref = personRef.Reference :> ITableReference

  let pliteRef = TableIdentifier<PersonLite>()
  let plite = pliteRef.Reference :> ITableReference

  let oldies = Table<PersonLite>.WithTableAlias("Oldies").ToTableReference()
  let younglings = Table<PersonLite>.WithTableAlias("YoungOnes").ToTableReference()
  let yref = younglings.Reference :> ITableReference
  let oref = oldies.Reference :> ITableReference
  // Hold your hat tight, cause these expressions gets hairy.


  let selectFromACteExpression =
    ({ Definitions = [ { TableDef = pliteRef
                         Query = { TestHelpers.EmptySelect with
                                    Select = { IsDistinct = false
                                               Values = [ ValueNode.Column("Name", typeof<string>, pref)
                                                          ValueNode.Column("Age", typeof<int>, pref) ] }
                                    From = { Tables = [ personRef ]; Joins = [] }
                                    Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, pref)
                                                        Operation = BinaryOperation.GreaterThan
                                                        Rhs = ValueNode.Constant("15") 
                                                      } |> ValueNode.BinaryExpression
                                              AdditionalClauses = []
                                            } |> Some } |> PlainSelectExpression.Plain
                        } ] }, 
      { TestHelpers.EmptySelect with 
          Select = { IsDistinct = false; Values = [ ValueNode.Column("Name", typeof<string>, plite) ] }
          From = { Tables = [ pliteRef ]; Joins = [] }
          Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, plite)
                              Operation = BinaryOperation.Equal
                              Rhs = ValueNode.Constant("42") 
                            } |> ValueNode.BinaryExpression 
                    AdditionalClauses = [] 
                  } |> Some 
      } |> PlainSelectExpression.Plain) 
    |> SelectExpression.Complex 
    |> SqlExpression.Select

  let selectFromSeveralCtesExpression =
    ({ Definitions = [ { TableDef = younglings
                         Query = { TestHelpers.EmptySelect with
                                        Select = { IsDistinct = false
                                                   Values = [ ValueNode.Column("Name", typeof<string>, pref)
                                                              ValueNode.Column("Age", typeof<int>, pref) ] }
                                        From = { Tables = [ personRef ]; Joins = [] }
                                        Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, pref)
                                                            Operation = BinaryOperation.LessThanOrEqual
                                                            Rhs = ValueNode.Constant("15") 
                                                          } |> ValueNode.BinaryExpression 
                                                  AdditionalClauses = [] } |> Some } |> PlainSelectExpression.Plain } 
                       { TableDef = oldies
                         Query = { TestHelpers.EmptySelect with
                                        Select = { IsDistinct = false
                                                   Values = [ ValueNode.Column("Name", typeof<string>, pref)
                                                              ValueNode.Column("Age", typeof<int>, pref) ] }
                                        From = { Tables = [ personRef ]; Joins = [] }
                                        Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, pref)
                                                            Operation = BinaryOperation.GreaterThan
                                                            Rhs = ValueNode.Constant("40") 
                                                          } |> ValueNode.BinaryExpression 
                                                  AdditionalClauses = [] } |> Some } |> PlainSelectExpression.Plain } ] },
      { TestHelpers.EmptySelect with
            Select = { IsDistinct = false
                       Values = [ ValueNode.Column("Name", typeof<string>, oref)
                                  ValueNode.Column("Name", typeof<string>, yref) ] } 
            From = { Tables = [ oldies ]
                     Joins = [ { SourceTables = [ oldies ]
                                 TargetTable = younglings
                                 Type = JoinType.InnerJoin
                                 Condition = { Lhs = { Lhs = ValueNode.Column("Age", typeof<int>, oref)
                                                       Operation = BinaryOperation.Subtract
                                                       Rhs = ValueNode.Constant("30") } |> ValueNode.BinaryExpression
                                               Operation = BinaryOperation.Equal
                                               Rhs = ValueNode.Column("Age", typeof<int>, yref) 
                                             } |> ValueNode.BinaryExpression |> Some } 
                              ] } 
      } |> PlainSelectExpression.Plain)
    |> SelectExpression.Complex 
    |> SqlExpression.Select

  let plitePagingRef = TableIdentifier<PersonLitePagingResult>()
  let plitepaging = plitePagingRef.Reference :> ITableReference

  let commonPagingExpression =
    ({ Definitions = 
        [ { TableDef = plitePagingRef
            Query = { TestHelpers.EmptySelect with
                        Select = 
                          { IsDistinct = false
                            Values = [ ValueNode.Column("Name", typeof<string>, pref)
                                       ValueNode.Column("Age", typeof<int>, pref)
                                       ValueNode.WindowedColumn(
                                         (AggregateType.RowNumber, ValueNode.NullValue), 
                                         { PartitionBy = []
                                           OrderBy = 
                                             [ { Direction = Ascending
                                                 NullsOrdering = NullsOrdering.NullsUndefined
                                                 Selector = ValueNode.Column("Age", typeof<int>, pref) } ] }) 
                                             ] }
                        From = { Tables = [ personRef ]
                                 Joins = [] }
                    } |> PlainSelectExpression.Plain
                  
          } ] }, 
     { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.Column("Name", typeof<string>, plitepaging)
                              ValueNode.Column("Age", typeof<int>, plitepaging) ] }
        From = { Tables = [ plitePagingRef ]; Joins = [] }
        Where = { Start = { Operation = BinaryOperation.And
                            Lhs = { Operation = BinaryOperation.GreaterThanOrEqual
                                    Lhs = ValueNode.Column("RowNumber", typeof<int>, plitepaging)
                                    Rhs = ValueNode.Constant("10")
                                  } |> ValueNode.BinaryExpression
                            Rhs = { Operation = BinaryOperation.LessThan
                                    Lhs = ValueNode.Column("RowNumber", typeof<int>, plitepaging)
                                    Rhs = ValueNode.Constant("20")
                                  } |> ValueNode.BinaryExpression
                          } |> ValueNode.BinaryExpression
                  AdditionalClauses = [] 
                } |> Some
     } |> PlainSelectExpression.Plain)
    |> SelectExpression.Complex
    |> SqlExpression.Select

  let recursivePersonRef = TableIdentifier<RecursivePerson>()
  let rpref = recursivePersonRef.Reference :> ITableReference

  let recursiveCommonTableExpression =
    ({ Definitions = 
        [ { TableDef = recursivePersonRef
            Query = 
              { Operation = SetOperationType.UnionAll
                Operands = 
                  [ { TestHelpers.EmptySelect with 
                        Select = { IsDistinct = false
                                   Values = 
                                      [ ValueNode.NamedColumn({ Alias = "Level"
                                                                Column = ValueNode.Constant("0") })
                                        ValueNode.Column("Name", typeof<string>, pref)
                                        ValueNode.Column("ParentId", typeof<int>, pref)
                                      ] }
                        From = { Tables = [ personRef ]; Joins = [] }
                        Where = { Start = { Operation = BinaryOperation.Equal
                                            Lhs = ValueNode.Column("Name", typeof<string>, pref)
                                            Rhs = ValueNode.Constant("'Kalle'")
                                          } |> ValueNode.BinaryExpression
                                  AdditionalClauses = [] 
                                } |> Some
                    } |> PlainSelectExpression.Plain 
                    { TestHelpers.EmptySelect with
                        Select = { IsDistinct = false
                                   Values = 
                                      [ ValueNode.Column("Name", typeof<string>, pref)
                                        ValueNode.Column("ParentId", typeof<int>, pref)
                                        ValueNode.NamedColumn(
                                          { Alias = "Level"
                                            Column = { Operation = BinaryOperation.Add
                                                       Lhs = ValueNode.Column("Level", typeof<int>, rpref)
                                                       Rhs = ValueNode.Constant("1")
                                                     } |> ValueNode.BinaryExpression 
                                          })
                                      ] }
                        From = { Tables = [ personRef ]
                                 Joins = [ { SourceTables = [ personRef ]
                                             TargetTable = recursivePersonRef
                                             Type = JoinType.InnerJoin
                                             Condition = { Operation = BinaryOperation.Equal
                                                           Lhs = ValueNode.Column("Id", typeof<int>, pref)
                                                           Rhs = ValueNode.Column("ParentId", typeof<int>, rpref)
                                                         } |> ValueNode.BinaryExpression |> Some
                                           } ] }
                    } |> PlainSelectExpression.Plain
                  ] 
              } |> PlainSelectExpression.Set
          } ] }, 
     { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.StarColumn(rpref) ] }
        From = { Tables = [ recursivePersonRef ]; Joins = [] }
     } |> PlainSelectExpression.Plain)
    |> SelectExpression.Complex
    |> SqlExpression.Select