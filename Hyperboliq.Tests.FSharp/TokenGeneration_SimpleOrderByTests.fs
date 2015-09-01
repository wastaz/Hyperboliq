namespace Hyperboliq.Tests.TokenGeneration

module SimpleOrderByTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq
    open Hyperboliq.Domain.AST
    open Hyperboliq.Domain

    [<Test>]
    let ``It should be possible to order ascedning by a column`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .OrderBy(<@ fun (p : Person) -> p.Age @>, Direction.Ascending)
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected =
            { TestHelpers.EmptySelect with
                Select = { IsDistinct = false
                           Values = [ ValueNode.StarColumn(tref.Reference) ] }
                From = { Tables = [ tref ]; Joins = [] }
                OrderBy = { OrderByExpressionNode.Clauses = 
                                [ { Direction = Direction.Ascending
                                    NullsOrdering = NullsOrdering.NullsUndefined
                                    Selector = ValueNode.Column("Age", typeof<int>, tref.Reference :> ITableReference) } ] } |> Some
            } |> TestHelpers.ToPlainSelect
        result |> should equal expected