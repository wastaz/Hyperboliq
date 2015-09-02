namespace Hyperboliq.Tests.TokenGeneration

module SimpleOrderByTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq
    open Hyperboliq.Domain.AST
    open Hyperboliq.Domain

    let directions = [ [| Direction.Ascending |]; [| Direction.Descending |] ]

    [<Test>]
    [<TestCaseSource("directions")>]
    let ``It should be possible to specify the order direction`` direction =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .OrderBy(<@ fun (p : Person) -> p.Age @>, direction)
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected =
            { TestHelpers.EmptySelect with
                Select = { IsDistinct = false
                           Values = [ ValueNode.StarColumn(tref.Reference) ] }
                From = { Tables = [ tref ]; Joins = [] }
                OrderBy = { OrderByExpressionNode.Clauses = 
                                [ { Direction = direction
                                    NullsOrdering = NullsOrdering.NullsUndefined
                                    Selector = ValueNode.Column("Age", typeof<int>, tref.Reference :> ITableReference) } ] } |> Some
            } |> TestHelpers.ToPlainSelect
        result |> should equal expected

    [<Test>]
    let ``It should be possible to order by several columns`` () =
        let expr = Select.Star<Person>().Star<Car>()
                         .From<Person>()
                         .InnerJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
                         .OrderBy(<@ fun (p : Person) -> p.Age @>, Direction.Ascending)
                         .ThenBy(<@ fun (c : Car) -> c.Brand @>, Direction.Descending)
        let result = expr.ToSqlExpression()

        let pref = TableIdentifier<Person>()
        let cref = TableIdentifier<Car>()
        let expected =
            { TestHelpers.EmptySelect with
                Select = { IsDistinct = false
                           Values = [ ValueNode.StarColumn(cref.Reference); ValueNode.StarColumn(pref.Reference) ] }
                From = { Tables = [ pref ]
                         Joins = [ { SourceTables = [ pref ]
                                     TargetTable = cref
                                     Type = JoinType.InnerJoin
                                     Condition = { Operation = BinaryOperation.Equal
                                                   Lhs = ValueNode.Column("Id", typeof<int>, pref.Reference :> ITableReference)
                                                   Rhs = ValueNode.Column("DriverId", typeof<int>, cref.Reference :> ITableReference) } |> ValueNode.BinaryExpression |> Some } ] }
                OrderBy = { OrderByExpressionNode.Clauses = 
                                [ { Direction = Descending
                                    NullsOrdering = NullsOrdering.NullsUndefined
                                    Selector = ValueNode.Column("Brand", typeof<string>, cref.Reference :> ITableReference) }
                                  { Direction = Ascending
                                    NullsOrdering = NullsOrdering.NullsUndefined
                                    Selector = ValueNode.Column("Age", typeof<int>, pref.Reference :> ITableReference) } ] } |> Some
            } |> TestHelpers.ToPlainSelect
        result |> should equal expected


    // According to ANSI SQL the order by clause should support the keywords NULLS FIRST/NULLS LAST
    // after ASC/DESC in an ordering clause. However some DBMS'es does not implement this part of the standard...
    // But some do!
    let nullsOrdering = [ [| NullsOrdering.NullsFirst |]; [| NullsOrdering.NullsLast|]; [| NullsOrdering.NullsUndefined |] ]

    [<Test>]
    [<TestCaseSource("nullsOrdering")>]
    let ``It should be possible to specify nulls ordering`` nullsOrder =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .OrderBy(<@ fun (p : Person) -> p.Age @>, Direction.Ascending, nullsOrder)
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected =
            { TestHelpers.EmptySelect with
                Select = { IsDistinct = false
                           Values = [ ValueNode.StarColumn(tref.Reference) ] } 
                From = { Tables = [ tref ]; Joins = [] }
                OrderBy = { OrderByExpressionNode.Clauses = 
                                [ { Direction = Direction.Ascending
                                    NullsOrdering = nullsOrder
                                    Selector = ValueNode.Column("Age", typeof<int>, tref.Reference :> ITableReference) } ] } |> Some
            } |> TestHelpers.ToPlainSelect
        result |> should equal expected

    [<Test>]
    let ``It should be possible to specify different nulls ordering on each column`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .OrderBy(<@ fun (p : Person) -> p.Age @>, Direction.Ascending, NullsOrdering.NullsLast)
                         .ThenBy(<@ fun (p : Person) -> p.Name @>, Direction.Descending, NullsOrdering.NullsFirst)
        let result = expr.ToSqlExpression()
        
        let tref = TableIdentifier<Person>()
        let expected = 
            { TestHelpers.EmptySelect with
                Select = { IsDistinct = false
                           Values = [ ValueNode.StarColumn(tref.Reference) ] }
                From = { Tables = [ tref ]; Joins = [] }
                OrderBy = { OrderByExpressionNode.Clauses =
                                [ { Direction = Direction.Descending
                                    NullsOrdering = NullsOrdering.NullsFirst
                                    Selector = ValueNode.Column("Name", typeof<string>, tref.Reference :> ITableReference) }
                                  { Direction = Direction.Ascending
                                    NullsOrdering = NullsOrdering.NullsLast
                                    Selector = ValueNode.Column("Age", typeof<int>, tref.Reference :> ITableReference) } ] } |> Some
            } |> TestHelpers.ToPlainSelect
        result |> should equal expected