namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SetOperationTests_Results =
  open Hyperboliq
  open Hyperboliq.Domain.AST
  open Hyperboliq.Domain

  let personRef = TableIdentifier<Person>()
  let pref = personRef.Reference :> ITableReference

  let simpleUnionExpression =
    { Operation = SetOperationType.Union
      Operands = 
        [ { TestHelpers.EmptySelect with
              Select = { IsDistinct = false 
                         Values = [ ValueNode.StarColumn(pref) ] }
              From = { Tables = [ personRef ]; Joins = [] }
              Where = { Start = { Operation = BinaryOperation.GreaterThan
                                  Lhs = ValueNode.Column("Age", typeof<int>, pref)
                                  Rhs = ValueNode.Constant("42")
                                } |> ValueNode.BinaryExpression
                        AdditionalClauses = []
                      } |> Some
          } |> PlainSelectExpression.Plain
          { TestHelpers.EmptySelect with
              Select = { IsDistinct = false
                         Values = [ ValueNode.StarColumn(pref) ] } 
              From = { Tables = [ personRef ]; Joins = [] }
              Where = { Start = { Operation = BinaryOperation.Equal
                                  Lhs = ValueNode.Column("Name", typeof<string>, pref)
                                  Rhs = ValueNode.Constant("'Kalle'") 
                                } |> ValueNode.BinaryExpression
                        AdditionalClauses = []
                      } |> Some
          } |> PlainSelectExpression.Plain
        ] 
    } |> PlainSelectExpression.Set
    |> SelectExpression.Plain
    |> SqlExpression.Select

  let simpleUnionAllExpression =
    { Operation = SetOperationType.UnionAll
      Operands = 
        [ { TestHelpers.EmptySelect with
              Select = { IsDistinct = false 
                         Values = [ ValueNode.StarColumn(pref) ] }
              From = { Tables = [ personRef ]; Joins = [] }
              Where = { Start = { Operation = BinaryOperation.GreaterThan
                                  Lhs = ValueNode.Column("Age", typeof<int>, pref)
                                  Rhs = ValueNode.Constant("42")
                                } |> ValueNode.BinaryExpression
                        AdditionalClauses = []
                      } |> Some
          } |> PlainSelectExpression.Plain
          { TestHelpers.EmptySelect with
              Select = { IsDistinct = false
                         Values = [ ValueNode.StarColumn(pref) ] } 
              From = { Tables = [ personRef ]; Joins = [] }
              Where = { Start = { Operation = BinaryOperation.Equal
                                  Lhs = ValueNode.Column("Name", typeof<string>, pref)
                                  Rhs = ValueNode.Constant("'Kalle'") 
                                } |> ValueNode.BinaryExpression
                        AdditionalClauses = []
                      } |> Some
          } |> PlainSelectExpression.Plain
        ] 
    } |> PlainSelectExpression.Set
    |> SelectExpression.Plain
    |> SqlExpression.Select

  let simpleIntersectExpression =
    { Operation = SetOperationType.Intersect
      Operands = 
        [ { TestHelpers.EmptySelect with
              Select = { IsDistinct = false 
                         Values = [ ValueNode.StarColumn(pref) ] }
              From = { Tables = [ personRef ]; Joins = [] }
          } |> PlainSelectExpression.Plain
          { TestHelpers.EmptySelect with
              Select = { IsDistinct = false
                         Values = [ ValueNode.StarColumn(pref) ] } 
              From = { Tables = [ personRef ]; Joins = [] }
              Where = { Start = { Operation = BinaryOperation.GreaterThan
                                  Lhs = ValueNode.Column("Age", typeof<int>, pref)
                                  Rhs = ValueNode.Constant("42")
                                } |> ValueNode.BinaryExpression
                        AdditionalClauses = []
                      } |> Some
          } |> PlainSelectExpression.Plain
        ] 
    } |> PlainSelectExpression.Set
    |> SelectExpression.Plain
    |> SqlExpression.Select

  
  let simpleMinusExpression =
    { Operation = SetOperationType.Minus
      Operands = 
        [ { TestHelpers.EmptySelect with
              Select = { IsDistinct = false 
                         Values = [ ValueNode.StarColumn(pref) ] }
              From = { Tables = [ personRef ]; Joins = [] }
          } |> PlainSelectExpression.Plain
          { TestHelpers.EmptySelect with
              Select = { IsDistinct = false
                         Values = [ ValueNode.StarColumn(pref) ] } 
              From = { Tables = [ personRef ]; Joins = [] }
              Where = { Start = { Operation = BinaryOperation.GreaterThan
                                  Lhs = ValueNode.Column("Age", typeof<int>, pref)
                                  Rhs = ValueNode.Constant("42")
                                } |> ValueNode.BinaryExpression
                        AdditionalClauses = []
                      } |> Some
          } |> PlainSelectExpression.Plain
        ] 
    } |> PlainSelectExpression.Set
    |> SelectExpression.Plain
    |> SqlExpression.Select


  let cteRef = Table<Person>.WithTableAlias("cte").ToTableReference()
  let cte = cteRef.Reference :> ITableReference

  let unionsInACommonTableExpression = 
    ({ Definitions = 
        [ { TableDef = cteRef
            Query = 
              { Operation = SetOperationType.Union
                Operands =
                  [ { TestHelpers.EmptySelect with
                        Select = { IsDistinct = false
                                   Values = [ ValueNode.StarColumn(pref) ]
                                 }
                        From = { Tables = [ personRef ]; Joins = [] }
                        Where = { Start = { Operation = BinaryOperation.GreaterThan
                                            Lhs = ValueNode.Column("Age", typeof<int>, pref)
                                            Rhs = ValueNode.Constant("42") 
                                          } |> ValueNode.BinaryExpression
                                  AdditionalClauses = []
                                } |> Some
                    } |> PlainSelectExpression.Plain
                    { TestHelpers.EmptySelect with
                        Select = { IsDistinct = false
                                   Values = [ ValueNode.StarColumn(pref) ]
                                 }
                        From = { Tables = [ personRef ]; Joins = [] }
                        Where = { Start = { Operation = BinaryOperation.Equal
                                            Lhs = ValueNode.Column("Name", typeof<string>, pref)
                                            Rhs = ValueNode.Constant("'Kalle'") 
                                          } |> ValueNode.BinaryExpression
                                  AdditionalClauses = []
                                } |> Some
                    } |> PlainSelectExpression.Plain
                  ] 
              } |> PlainSelectExpression.Set
          } ]
     },
     { Operation = SetOperationType.Union
       Operands =
        [ { TestHelpers.EmptySelect with 
              Select = { IsDistinct = false
                         Values = [ ValueNode.StarColumn(cte) ]
                       }
              From = { Tables = [ cteRef ]; Joins = [] }
              Where = { Start = { Operation = BinaryOperation.Equal
                                  Lhs = ValueNode.Column("Age", typeof<int>, cte)
                                  Rhs = ValueNode.Constant("50") 
                                } |> ValueNode.BinaryExpression
                        AdditionalClauses = []
                      } |> Some
          } |> PlainSelectExpression.Plain
          { TestHelpers.EmptySelect with 
              Select = { IsDistinct = false
                         Values = [ ValueNode.StarColumn(cte) ]
                       }
              From = { Tables = [ cteRef ]; Joins = [] }
              Where = { Start = { Operation = BinaryOperation.Equal
                                  Lhs = ValueNode.Column("Name", typeof<string>, cte)
                                  Rhs = ValueNode.Constant("'Kalle'") 
                                } |> ValueNode.BinaryExpression
                        AdditionalClauses = []
                      } |> Some
          } |> PlainSelectExpression.Plain
        ]
     } |> PlainSelectExpression.Set)
    |> SelectExpression.Complex
    |> SqlExpression.Select


