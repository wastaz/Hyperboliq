namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_ParametrizedQueryTests_Result =
  open Hyperboliq
  open Hyperboliq.Domain.AST
  open Hyperboliq.Domain

  let personRef = TableIdentifier<Person>()
  let carRef = TableIdentifier<Car>()
  
  let oneOffParameterExpression = 
    { TestHelpers.EmptySelect with 
        Select = { IsDistinct = false
                   Values = [ ValueNode.StarColumn(personRef.Reference) ] }
        From = { Tables = [ personRef ]; Joins = [] }
        Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, personRef.Reference :> ITableReference)
                            Operation = BinaryOperation.LessThan
                            Rhs = ValueNode.Parameter("age") } |> ValueNode.BinaryExpression
                  AdditionalClauses = []
                } |> Some
    } |> TestHelpers.ToPlainSelect

  let parameterizedExpression =
    { TestHelpers.EmptySelect with 
        Select = { IsDistinct = false
                   Values = [ ValueNode.StarColumn(personRef.Reference) ] }
        From = { Tables = [ personRef ]; Joins = [] }
        Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, personRef.Reference :> ITableReference)
                            Operation = BinaryOperation.GreaterThan
                            Rhs = ValueNode.Parameter("age") } |> ValueNode.BinaryExpression
                  AdditionalClauses = []
                } |> Some
    } |> TestHelpers.ToPlainSelect

  let complexParameterizedExpression =
    { TestHelpers.EmptySelect with 
        Select = { IsDistinct = false
                   Values = [ ValueNode.StarColumn(personRef.Reference) ] }
        From = { Tables = [ personRef ]; Joins = [] }
        Where = { Start = { Lhs = { Lhs = ValueNode.Column("Age", typeof<int>, personRef.Reference :> ITableReference)
                                    Operation = BinaryOperation.GreaterThan
                                    Rhs = ValueNode.Parameter("age")
                                  } |> ValueNode.BinaryExpression 
                            Operation = BinaryOperation.And
                            Rhs = { Lhs = ValueNode.Parameter("age")
                                    Operation = BinaryOperation.LessThan
                                    Rhs = ValueNode.Constant("90")
                                  } |> ValueNode.BinaryExpression } |> ValueNode.BinaryExpression
                  AdditionalClauses = [ { Combinator = ExpressionCombinatorType.Or 
                                          Expression = { Lhs = ValueNode.Column("Age", typeof<int>, personRef.Reference :> ITableReference)
                                                         Operation = BinaryOperation.LessThan
                                                         Rhs = ValueNode.Parameter("age") } |> ValueNode.BinaryExpression } ]
                } |> Some
    } |> TestHelpers.ToPlainSelect