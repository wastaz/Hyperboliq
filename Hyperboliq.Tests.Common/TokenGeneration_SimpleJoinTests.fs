namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SimpleJoinTests_Results =
  open Hyperboliq
  open Hyperboliq.Domain.AST
  open Hyperboliq.Domain

  let personRef = TableIdentifier<Person>()
  let carRef = TableIdentifier<Car>()
  let houseRef = TableIdentifier<House>()

  let expectedAst joinType =
      { TestHelpers.EmptySelect with
          Select = { IsDistinct = false
                     Values = [ ValueNode.StarColumn(personRef.Reference) ] }
          From = { Tables = [ personRef ]
                   Joins = [ { SourceTables = [ personRef ]
                               TargetTable = carRef
                               Type = joinType
                               Condition = { Operation = BinaryOperation.Equal
                                             Lhs = ValueNode.Column("Id", typeof<int>, personRef.Reference :> ITableReference)
                                             Rhs = ValueNode.Column("DriverId", typeof<int>, carRef.Reference :> ITableReference) } |> ValueNode.BinaryExpression |> Some } ] }
      } |> TestHelpers.ToPlainSelect


  let multipleJoinExpression =
    { TestHelpers.EmptySelect with
        Select = { IsDistinct = false
                   Values = [ ValueNode.StarColumn(houseRef.Reference); ValueNode.StarColumn(carRef.Reference); ValueNode.StarColumn(personRef.Reference) ] } 
        From = { Tables = [ houseRef ]
                 Joins = [ { SourceTables = [ personRef ] 
                             TargetTable = carRef
                             Type = JoinType.LeftJoin
                             Condition = { Operation = BinaryOperation.Equal
                                           Lhs = ValueNode.Column("Id", typeof<int>, personRef.Reference :> ITableReference)
                                           Rhs = ValueNode.Column("DriverId", typeof<int>, carRef.Reference :> ITableReference) } |> ValueNode.BinaryExpression |> Some } 
                           { SourceTables = [ houseRef ]
                             TargetTable = personRef
                             Type = JoinType.InnerJoin
                             Condition = { Operation = BinaryOperation.Equal
                                           Lhs = ValueNode.Column("Id", typeof<int>, houseRef.Reference :> ITableReference)
                                           Rhs = ValueNode.Column("LivesAtHouseId", typeof<int>, personRef.Reference :> ITableReference) } |> ValueNode.BinaryExpression |> Some } ] }
    } |> TestHelpers.ToPlainSelect