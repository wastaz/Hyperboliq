namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SelectPartitionTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq
    open Hyperboliq.Domain.AST
    open Hyperboliq.Domain

    [<Test>]
    let ``It should be possible to use an empty over clause`` () =
        let expr = Select.Column(<@ fun (p : Person) -> p.Name @>)
                         .Column(<@ fun (p : Person) -> Sql.Sum(p.Age) @>, Over.Empty)
                         .From<Person>()
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected =
            { TestHelpers.EmptySelect with
                Select = { IsDistinct = false
                           Values = [ ValueNode.Column("Name", typeof<string>, tref.Reference :> ITableReference)
                                      ValueNode.WindowedColumn((AggregateType.Sum, ValueNode.Column("Age", typeof<int>, tref.Reference :> ITableReference)), 
                                                               { PartitionBy = []; OrderBy = [] }) ] }
                From = { Tables = [ tref ]; Joins = [] }
            } |> TestHelpers.ToPlainSelect
        result |> should equal expected
