namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SimpleJoinTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq
    open Hyperboliq.Domain.AST
    open Hyperboliq.Domain

    let expectedStream joinType =
        let pref = TableIdentifier<Person>()
        let cref = TableIdentifier<Car>()
        { TestHelpers.EmptySelect with
            Select = { IsDistinct = false
                       Values = [ ValueNode.StarColumn(pref.Reference) ] }
            From = { Tables = [ pref ]
                     Joins = [ { SourceTables = [ pref ]
                                 TargetTable = cref
                                 Type = joinType
                                 Condition = { Operation = BinaryOperation.Equal
                                               Lhs = ValueNode.Column("Id", typeof<int>, pref.Reference :> ITableReference)
                                               Rhs = ValueNode.Column("DriverId", typeof<int>, cref.Reference :> ITableReference) } |> ValueNode.BinaryExpression |> Some } ] }
        } |> TestHelpers.ToPlainSelect

    [<Test>]
    let ``It should be able to perform a simple inner join`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .InnerJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
        let result = expr.ToSqlExpression()
        let expected = expectedStream JoinType.InnerJoin
        result |> should equal expected

    [<Test>]
    let ``It should be able to perform a simple left join`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .LeftJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
        let result = expr.ToSqlExpression()
        let expected = expectedStream JoinType.LeftJoin
        result |> should equal expected

    [<Test>]
    let ``It should be able to perform a simple right join`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .RightJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
        let result = expr.ToSqlExpression()
        let expected = expectedStream JoinType.RightJoin
        result |> should equal expected
    
    [<Test>]
    let ``It should be able to perform a simple full outer join`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .FullJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
        let result = expr.ToSqlExpression()
        let expected = expectedStream JoinType.FullJoin
        result |> should equal expected
        
    [<Test>]
    let ``It should be able to perform multiple joins`` () =
        let expr = Select.Star<Person>().Star<Car>().Star<House>()
                         .From<House>()
                         .InnerJoin(<@ fun (h : House) (p : Person) -> h.Id = p.LivesAtHouseId @>)
                         .LeftJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
        let result = expr.ToSqlExpression()

        let pref, cref, href = TableIdentifier<Person>(), TableIdentifier<Car>(), TableIdentifier<House>()
        let expected =
            { TestHelpers.EmptySelect with
                Select = { IsDistinct = false
                           Values = [ ValueNode.StarColumn(href.Reference); ValueNode.StarColumn(cref.Reference); ValueNode.StarColumn(pref.Reference) ] } 
                From = { Tables = [ href ]
                         Joins = [ { SourceTables = [ pref] 
                                     TargetTable = cref
                                     Type = JoinType.LeftJoin
                                     Condition = { Operation = BinaryOperation.Equal
                                                   Lhs = ValueNode.Column("Id", typeof<int>, pref.Reference :> ITableReference)
                                                   Rhs = ValueNode.Column("DriverId", typeof<int>, cref.Reference :> ITableReference) } |> ValueNode.BinaryExpression |> Some } 
                                   { SourceTables = [ href ]
                                     TargetTable = pref
                                     Type = JoinType.InnerJoin
                                     Condition = { Operation = BinaryOperation.Equal
                                                   Lhs = ValueNode.Column("Id", typeof<int>, href.Reference :> ITableReference)
                                                   Rhs = ValueNode.Column("LivesAtHouseId", typeof<int>, pref.Reference :> ITableReference) } |> ValueNode.BinaryExpression |> Some } ] }
            } |> TestHelpers.ToPlainSelect
        result |> should equal expected