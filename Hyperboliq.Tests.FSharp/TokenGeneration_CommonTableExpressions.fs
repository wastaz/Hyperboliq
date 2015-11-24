namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_CommonTableExpressions =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq.Tests.TokenGeneration.TestHelpers
    open Hyperboliq
    open Hyperboliq.Domain
    open Hyperboliq.Domain.AST

    [<Test>]
    let ``It should be possible to select from a common table expression`` () =
        let expr = 
            With.Table<PersonLite>(
                    Select.Column(<@ fun (p : Person) -> p.Name, p.Age @>)
                          .From<Person>()
                          .Where(<@ fun (p : Person) -> p.Age > 15 @>))
                .Query(
                    Select.Column(<@ fun (p : PersonLite) -> p.Name @>)
                          .From<PersonLite>()
                          .Where(<@ fun (p : PersonLite) -> p.Age = 42 @>))
        let result = expr.ToSqlExpression()

        let pref = TableIdentifier<Person>()
        let plref = TableIdentifier<PersonLite>()
        let expected = 
            ({ Definitions = [ { TableDef = plref
                                 Query = { TestHelpers.EmptySelect with
                                            Select = { IsDistinct = false
                                                       Values = [ ValueNode.Column("Name", typeof<string>, pref.Reference :> ITableReference)
                                                                  ValueNode.Column("Age", typeof<int>, pref.Reference :> ITableReference) ] }
                                            From = { Tables = [ pref ]; Joins = [] }
                                            Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, pref.Reference :> ITableReference)
                                                                Operation = BinaryOperation.GreaterThan
                                                                Rhs = ValueNode.Constant("15") 
                                                              } |> ValueNode.BinaryExpression
                                                      AdditionalClauses = []
                                                    } |> Some } |> PlainSelectExpression.Plain
                               } ] }, 
             { TestHelpers.EmptySelect with 
                Select = { IsDistinct = false; Values = [ ValueNode.Column("Name", typeof<string>, plref.Reference :> ITableReference) ] }
                From = { Tables = [ plref ]; Joins = [] }
                Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, plref.Reference :> ITableReference)
                                    Operation = BinaryOperation.Equal
                                    Rhs = ValueNode.Constant("42") 
                                  } |> ValueNode.BinaryExpression 
                          AdditionalClauses = [] 
                        } |> Some 
             } |> PlainSelectExpression.Plain) 
            |> SelectExpression.Complex 
            |> SqlExpression.Select
        result |> should equal expected

    [<Test>]
    let ``It should be possible to select from several common table expressions`` () =
        let oldies = Table<PersonLite>.WithTableAlias("Oldies")
        let younglings = Table<PersonLite>.WithTableAlias("YoungOnes")

        let expr =
            With.Table(
                    oldies,
                    Select.Column(<@ fun (p : Person) -> p.Name, p.Age @>)
                          .From<Person>()
                          .Where(<@ fun (p : Person) -> p.Age > 40 @>))
                .Table(
                    younglings,
                    Select.Column(<@ fun (p : Person) -> p.Name, p.Age @>)
                          .From<Person>()
                          .Where(<@ fun (p : Person) -> p.Age <= 15 @>))
                .Query(
                    Select.Column(oldies, <@ fun (p : PersonLite) -> p.Name @>)
                          .Column(younglings, <@ fun (p : PersonLite) -> p.Name @>)
                          .From(oldies)
                          .InnerJoin(oldies, younglings, <@ fun (old : PersonLite) (young : PersonLite) -> old.Age - 30 = young.Age @>))
        let result = expr.ToSqlExpression()
        
        let oref = oldies.ToTableReference()
        let yref = younglings.ToTableReference()
        let pref = TableIdentifier<Person>()
        let expected = 
            ({ Definitions = [ { TableDef = yref
                                 Query = { TestHelpers.EmptySelect with
                                                Select = { IsDistinct = false
                                                           Values = [ ValueNode.Column("Name", typeof<string>, pref.Reference :> ITableReference)
                                                                      ValueNode.Column("Age", typeof<int>, pref.Reference :> ITableReference) ] }
                                                From = { Tables = [ pref ]; Joins = [] }
                                                Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, pref.Reference :> ITableReference)
                                                                    Operation = BinaryOperation.LessThanOrEqual
                                                                    Rhs = ValueNode.Constant("15") 
                                                                  } |> ValueNode.BinaryExpression 
                                                          AdditionalClauses = [] } |> Some } |> PlainSelectExpression.Plain } 
                               { TableDef = oref
                                 Query = { TestHelpers.EmptySelect with
                                                Select = { IsDistinct = false
                                                           Values = [ ValueNode.Column("Name", typeof<string>, pref.Reference :> ITableReference)
                                                                      ValueNode.Column("Age", typeof<int>, pref.Reference :> ITableReference) ] }
                                                From = { Tables = [ pref ]; Joins = [] }
                                                Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, pref.Reference :> ITableReference)
                                                                    Operation = BinaryOperation.GreaterThan
                                                                    Rhs = ValueNode.Constant("40") 
                                                                  } |> ValueNode.BinaryExpression 
                                                          AdditionalClauses = [] } |> Some } |> PlainSelectExpression.Plain } ] },
             { TestHelpers.EmptySelect with
                    Select = { IsDistinct = false
                               Values = [ ValueNode.Column("Name", typeof<string>, oref.Reference :> ITableReference)
                                          ValueNode.Column("Name", typeof<string>, yref.Reference :> ITableReference) ] } 
                    From = { Tables = [ oldies ]
                             Joins = [ { SourceTables = [ oldies ]
                                         TargetTable = younglings
                                         Type = JoinType.InnerJoin
                                         Condition = { Lhs = { Lhs = ValueNode.Column("Age", typeof<int>, oref.Reference :> ITableReference)
                                                               Operation = BinaryOperation.Subtract
                                                               Rhs = ValueNode.Constant("30") } |> ValueNode.BinaryExpression
                                                       Operation = BinaryOperation.Equal
                                                       Rhs = ValueNode.Column("Age", typeof<int>, yref.Reference :> ITableReference) 
                                                     } |> ValueNode.BinaryExpression |> Some } 
                                     ] } 
             } |> PlainSelectExpression.Plain)
            |> SelectExpression.Complex 
            |> SqlExpression.Select
        result |> should equal expected