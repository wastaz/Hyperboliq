namespace Hyperboliq.Tests.TokenGeneration

module SimpleGroupByTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq
    open Hyperboliq.Domain.AST
    open Hyperboliq.Domain
    
    [<Test>]
    let ``It should be possible to group by a single column`` () =
        let expr = Select.Column(<@ fun (p : Person) -> let maxAge = Sql.Max(p.Age) in (p.Name, maxAge) @>)
                         .From<Person>()
                         .GroupBy(<@ fun (p : Person) -> p.Name @>)
        let result = expr.ToSqlExpression()
        
        let tref = TableIdentifier<Person>()
        let expected =
            { TestHelpers.EmptySelect with 
                Select = { IsDistinct = false
                           Values = [ ValueNode.Column("Name", typeof<string>, tref.Reference :> ITableReference)
                                      ValueNode.NamedColumn({ Alias = "maxAge"
                                                              Column = ValueNode.Aggregate(AggregateType.Max, ValueNode.Column("Age", typeof<int>, tref.Reference :> ITableReference)) }) ] } 
                From = { Tables = [ tref ]; Joins = [] }
                GroupBy = { Clauses = [ ValueNode.Column("Name", typeof<string>, tref.Reference :> ITableReference) ]
                            Having = [] } |> Some
            } |> TestHelpers.ToPlainSelect
        result |> should equal expected

    [<Test>]
    let ``It should be possible to group by multiple columns`` () =
        let expr = Select.Column(<@ fun (p : Person) -> let minAge = Sql.Min(p.Age) in (p.Name, p.LivesAtHouseId, minAge) @>)
                         .From<Person>()
                         .GroupBy(<@ fun (p : Person) -> (p.Name, p.LivesAtHouseId) @>)
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected = 
            { TestHelpers.EmptySelect with
                Select =  { IsDistinct = false
                            Values = [ ValueNode.Column("Name", typeof<string>, tref.Reference :> ITableReference)
                                       ValueNode.Column("LivesAtHouseId", typeof<int>, tref.Reference :> ITableReference)
                                       ValueNode.NamedColumn({ Alias = "minAge"
                                                               Column = ValueNode.Aggregate(AggregateType.Min, ValueNode.Column("Age", typeof<int>, tref.Reference :> ITableReference)) }) ] } 
                From = { Tables = [ tref ]; Joins = [] }
                GroupBy = { Clauses = [ ValueNode.Column("Name", typeof<string>, tref.Reference :> ITableReference)
                                        ValueNode.Column("LivesAtHouseId", typeof<int>, tref.Reference :> ITableReference) ]
                            Having = [] } |> Some
            } |> TestHelpers.ToPlainSelect
        result |> should equal expected

    let ``It should be possible to group by columns from multiple tables`` () =
        let expr = Select.Column(<@ fun (p : Person) -> let averageAge = Sql.Avg(p.Age) in (p.Name, averageAge) @>)
                         .Column(<@ fun (c : Car) -> let minAge = Sql.Min(c.Age) in (c.Brand, minAge) @>)
                         .From<Person>()
                         .InnerJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
                         .GroupBy(<@ fun (p : Person) -> p.Name @>)
                         .ThenBy(<@ fun (c : Car) -> c.Brand @>)
        let result = expr.ToSqlExpression()
        
        let pref = TableIdentifier<Person>()
        let cref = TableIdentifier<Person>()
        let expected = 
            { TestHelpers.EmptySelect with
                Select =  { IsDistinct = false
                            Values = [ ValueNode.Column("Brand", typeof<string>, cref.Reference :> ITableReference)
                                       ValueNode.NamedColumn({ Alias = "minAge"
                                                               Column = ValueNode.Aggregate(AggregateType.Min, ValueNode.Column("Age", typeof<int>, cref.Reference :> ITableReference)) })
                                       ValueNode.Column("Name", typeof<string>, pref.Reference :> ITableReference)
                                       ValueNode.NamedColumn({ Alias = "averageAge"
                                                               Column = ValueNode.Aggregate(AggregateType.Avg, ValueNode.Column("Age", typeof<int>, pref.Reference :> ITableReference)) }) ] } 
                From = { Tables = [ pref ]
                         Joins = [ { SourceTables = [ pref ]
                                     TargetTable = cref
                                     Type = JoinType.InnerJoin
                                     Condition = { Operation = BinaryOperation.Equal
                                                   Lhs = ValueNode.Column("Id", typeof<int>, pref.Reference :> ITableReference)
                                                   Rhs = ValueNode.Column("DriverId", typeof<int>, cref.Reference :> ITableReference) } |> ValueNode.BinaryExpression |> Some } ] }
                GroupBy = { Clauses = [ ValueNode.Column("Name", typeof<string>, pref.Reference :> ITableReference)
                                        ValueNode.Column("Brand", typeof<string>, cref.Reference :> ITableReference) ] 
                            Having = [] } |> Some
            } |> TestHelpers.ToPlainSelect
        result |> should equal expected

    [<Test>]
    let ``It should be possible to use a single having expression`` () =
        let expr = Select.Column(<@ fun (p : Person) -> let averageAge = Sql.Avg(p.Age) in (p.Name, averageAge) @>)
                         .From<Person>()
                         .GroupBy(<@ fun (p : Person) -> p.Name @>)
                         .Having(<@ fun (p : Person) -> Sql.Avg(p.Age) > 42 @>)
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected =
            { TestHelpers.EmptySelect with
                Select = { IsDistinct = false
                           Values = [ ValueNode.Column("Name", typeof<string>, tref.Reference :> ITableReference)
                                      ValueNode.NamedColumn({ Alias = "averageAge"
                                                              Column = ValueNode.Aggregate(AggregateType.Avg, ValueNode.Column("Age", typeof<int>, tref.Reference :> ITableReference)) }) ] } 
                From = { Tables = [ tref ]; Joins = [] }
                GroupBy = { Clauses = [ ValueNode.Column("Name", typeof<string>, tref.Reference :> ITableReference) ]
                            Having = [ { Combinator = ExpressionCombinatorType.And
                                         Expression = { Operation = BinaryOperation.GreaterThan
                                                        Lhs = ValueNode.Aggregate(AggregateType.Avg, ValueNode.Column("Age", typeof<int>, tref.Reference :> ITableReference))
                                                        Rhs = ValueNode.Constant("42") } |> ValueNode.BinaryExpression }  ] } |> Some
            } |> TestHelpers.ToPlainSelect
        result |> should equal expected

    [<Test>]
    let ``It should be possible to use multiple having expressions`` () =
        let expr = Select.Column(<@ fun (p : Person) -> let averageAge = Sql.Avg(p.Age) in (p.Name, averageAge) @>)
                         .Column(<@ fun (c : Car) -> let minAge = Sql.Min(c.Age) in (c.Brand, minAge) @>)
                         .From<Person>()
                         .InnerJoin(<@ fun (p : Person) (c : Car) -> p.Id = c.DriverId @>)
                         .GroupBy(<@ fun (p : Person) -> p.Name @>)
                         .ThenBy(<@ fun (c : Car) -> c.Brand @>)
                         .Having(<@ fun (p : Person) -> Sql.Avg(p.Age) > 42 @>)
                         .And(<@ fun (c : Car) -> Sql.Min(c.Age) > 2 @>)
        let result = expr.ToSqlExpression()

        let pref = TableIdentifier<Person>()
        let cref = TableIdentifier<Car>()
        let expected =
            { TestHelpers.EmptySelect with 
                Select = { IsDistinct = false
                           Values = [ ValueNode.Column("Brand", typeof<string>, cref.Reference :> ITableReference)
                                      ValueNode.NamedColumn({ Alias = "minAge"
                                                              Column = ValueNode.Aggregate(AggregateType.Min, ValueNode.Column("Age", typeof<int>, cref.Reference :> ITableReference)) })
                                      ValueNode.Column("Name", typeof<string>, pref.Reference :> ITableReference)
                                      ValueNode.NamedColumn({ Alias = "averageAge"
                                                              Column = ValueNode.Aggregate(AggregateType.Avg, ValueNode.Column("Age", typeof<int>, pref.Reference :> ITableReference)) }) ] }
                From = { Tables = [ pref ]
                         Joins = [ { SourceTables = [ pref ]
                                     TargetTable = cref
                                     Type = JoinType.InnerJoin
                                     Condition = { Operation = BinaryOperation.Equal
                                                   Lhs = ValueNode.Column("Id", typeof<int>, pref.Reference :> ITableReference)
                                                   Rhs = ValueNode.Column("DriverId", typeof<int>, cref.Reference :> ITableReference) } |> ValueNode.BinaryExpression |> Some } ] }
                GroupBy = { Clauses = [ ValueNode.Column("Name", typeof<string>, pref.Reference :> ITableReference)
                                        ValueNode.Column("Brand", typeof<string>, cref.Reference :> ITableReference) ]
                            Having = [ { Combinator = ExpressionCombinatorType.And 
                                         Expression = { Operation = BinaryOperation.GreaterThan
                                                        Lhs = ValueNode.Aggregate(AggregateType.Min, ValueNode.Column("Age", typeof<int>, cref.Reference :> ITableReference))
                                                        Rhs = ValueNode.Constant("2") } |> ValueNode.BinaryExpression }
                                       { Combinator = ExpressionCombinatorType.And
                                         Expression = { Operation = BinaryOperation.GreaterThan
                                                        Lhs = ValueNode.Aggregate(AggregateType.Avg, ValueNode.Column("Age", typeof<int>, pref.Reference :> ITableReference))
                                                        Rhs = ValueNode.Constant("42") } |> ValueNode.BinaryExpression } ] } |> Some
            } |> TestHelpers.ToPlainSelect
        result |> should equal expected
