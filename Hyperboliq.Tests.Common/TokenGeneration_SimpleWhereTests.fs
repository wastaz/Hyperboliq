namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SimpleWhereTests_Results =
  open Hyperboliq
  open Hyperboliq.Domain.AST
  open Hyperboliq.Domain

  let personRef = TableIdentifier<Person>()
  let pref = personRef.Reference :> ITableReference

  let carRef = TableIdentifier<Car>()
  let cref = carRef.Reference :> ITableReference

  let animalRef = TableIdentifier<Animal>()
  let aref = animalRef.Reference :> ITableReference

  let simpleWhereConditionExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.StarColumn(pref) ] }
        From = { Tables = [ personRef ]; Joins = [] }
        Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, pref)
                            Rhs = ValueNode.Constant("42")
                            Operation = BinaryOperation.GreaterThan 
                          } |> ValueNode.BinaryExpression
                  AdditionalClauses = [] } |> Some 
    } |> TestHelpers.ToPlainSelect

  let conditionalWithAndAndOrsExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.StarColumn(pref) ] }
        From = { Tables = [ personRef ]; Joins = [] }
        Where = { Start = { Operation = BinaryOperation.Or
                            Rhs = { Operation = BinaryOperation.And
                                    Lhs = { Operation = BinaryOperation.LessThan
                                            Lhs = ValueNode.Column("Age", typeof<int>, pref)
                                            Rhs = ValueNode.Constant("10")
                                          } |> ValueNode.BinaryExpression
                                    Rhs = { Operation = BinaryOperation.Equal
                                            Lhs = ValueNode.Column("Name", typeof<string>, pref)
                                            Rhs = ValueNode.Constant("'Karl'") 
                                          } |> ValueNode.BinaryExpression 
                                  } |> ValueNode.BinaryExpression
                            Lhs = { Operation = BinaryOperation.GreaterThan
                                    Lhs = ValueNode.Column("Age", typeof<int>, pref)
                                    Rhs = ValueNode.Constant("42") 
                                  } |> ValueNode.BinaryExpression
                          } |> ValueNode.BinaryExpression
                  AdditionalClauses = [] } |> Some }
    |> TestHelpers.ToPlainSelect

  let conditionalWithAndAndOrsOutsideExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.StarColumn(pref) ] }
        From = { Tables = [ personRef ]; Joins = [] }
        Where = { Start = { Operation = BinaryOperation.LessThan
                            Lhs = ValueNode.Column("Age", typeof<int>, pref)
                            Rhs = ValueNode.Constant("42") } |> ValueNode.BinaryExpression 
                  AdditionalClauses = 
                  [ { Combinator = ExpressionCombinatorType.Or
                      Expression = { Operation = BinaryOperation.Equal
                                     Lhs = ValueNode.Column("Name", typeof<string>, pref)
                                     Rhs = ValueNode.Constant("'Karl'") } |> ValueNode.BinaryExpression } 
                    { Combinator = ExpressionCombinatorType.And
                      Expression = { Operation = BinaryOperation.GreaterThan
                                     Lhs = ValueNode.Column("Age", typeof<int>, pref)
                                     Rhs = ValueNode.Constant("12") } |> ValueNode.BinaryExpression }
                  ]
                } |> Some 
    } |> TestHelpers.ToPlainSelect

  let conditionsOnJoinedTablesExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.StarColumn(cref); ValueNode.StarColumn(pref) ] }
        From = { Tables = [ personRef ]
                 Joins = [ { SourceTables = [ personRef ]
                             TargetTable = carRef
                             Type = JoinType.InnerJoin
                             Condition = { Operation = BinaryOperation.Equal
                                           Lhs = ValueNode.Column("Id", typeof<int>, pref)
                                           Rhs = ValueNode.Column("DriverId", typeof<int>, cref) 
                                         } |> ValueNode.BinaryExpression |> Some } ] }
        Where = { Start = { Operation = BinaryOperation.GreaterThan
                            Lhs = ValueNode.Column("Age", typeof<int>, pref)
                            Rhs = ValueNode.Constant("42") } |> ValueNode.BinaryExpression
                  AdditionalClauses = [ { Combinator = ExpressionCombinatorType.And
                                          Expression = { Operation = BinaryOperation.Equal
                                                         Lhs = ValueNode.Column("Brand", typeof<string>, cref)
                                                         Rhs = ValueNode.Constant("'SAAB'") 
                                                       } |> ValueNode.BinaryExpression } ] } |> Some
        } |> TestHelpers.ToPlainSelect

  let conditionalReferencingSeveralTablesExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ StarColumn(cref); StarColumn(pref) ] }
        From = { Tables = [ personRef ]
                 Joins = [ { SourceTables = [ personRef ]
                             TargetTable = carRef
                             Type = JoinType.InnerJoin
                             Condition = { Operation = BinaryOperation.Equal
                                           Lhs = ValueNode.Column("Id", typeof<int>, pref)
                                           Rhs = ValueNode.Column("DriverId", typeof<int>, cref) 
                                         } |> ValueNode.BinaryExpression |> Some } ] }
        Where = { Start = { Operation = BinaryOperation.GreaterThan 
                            Lhs = ValueNode.Column("Age", typeof<int>, pref)
                            Rhs = ValueNode.Column("DriverId", typeof<int>, cref) 
                          } |> ValueNode.BinaryExpression
                  AdditionalClauses = [] } |> Some
    } |> TestHelpers.ToPlainSelect

  let conditionalReferencingSeveralTablesWithAndAndOrsExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false; Values = [ StarColumn(cref); StarColumn(pref) ] }
        From = { Tables = [ personRef ]
                 Joins = [ { SourceTables = [ personRef ]
                             TargetTable = carRef
                             Type = JoinType.InnerJoin
                             Condition = { Operation = BinaryOperation.Equal
                                           Lhs = ValueNode.Column("Id", typeof<int>, pref)
                                           Rhs = ValueNode.Column("DriverId", typeof<int>, cref) 
                                         } |> ValueNode.BinaryExpression |> Some } ] }
        Where = { Start = { Operation = BinaryOperation.GreaterThan
                            Lhs = ValueNode.Column("Age", typeof<int>, pref)
                            Rhs = ValueNode.Constant("42") } |> ValueNode.BinaryExpression
                  AdditionalClauses = [ { Combinator = ExpressionCombinatorType.Or
                                          Expression = { Operation = BinaryOperation.Equal
                                                         Lhs = ValueNode.Column("Name", typeof<string>, pref)
                                                         Rhs = ValueNode.Column("Brand", typeof<string>, cref) 
                                                       } |> ValueNode.BinaryExpression }
                                        { Combinator = ExpressionCombinatorType.And 
                                          Expression = { Operation = BinaryOperation.GreaterThan 
                                                         Lhs = ValueNode.Column("Age", typeof<int>, pref)
                                                         Rhs = ValueNode.Column("Age", typeof<int>, cref) 
                                                       } |> ValueNode.BinaryExpression } ] } |> Some
    } |> TestHelpers.ToPlainSelect

  let guidInConditionalExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false; Values = [ StarColumn(aref) ] }
        From = { Tables = [ animalRef ]; Joins = [] }
        Where = { Start = { Operation = BinaryOperation.Equal
                            Lhs = ValueNode.Column("Id", typeof<System.Guid>, aref)
                            Rhs = ValueNode.Constant("'7ba52264-9b79-41da-975a-5bc7980f08c1'") } |> ValueNode.BinaryExpression
                  AdditionalClauses = [] 
                } |> Some
    } |> TestHelpers.ToPlainSelect
    
  let dateInConditionalExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false; Values = [ StarColumn(aref) ] }
        From = { Tables = [ animalRef ]; Joins = [] }
        Where = { Start = { Operation = BinaryOperation.Equal
                            Lhs = ValueNode.Column("BornAt", typeof<System.DateTime>, aref)
                            Rhs = ValueNode.Constant("'2017-02-14T12:34:50.0000000'") } |> ValueNode.BinaryExpression
                  AdditionalClauses = [] 
                } |> Some
    } |> TestHelpers.ToPlainSelect