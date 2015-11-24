namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SelectPartitionTests =
    open NUnit.Framework
    open FsUnit
    open Hyperboliq
    open Hyperboliq.Domain.AST
    open Hyperboliq.Domain
    open Hyperboliq.Tests.TokenGeneration.TokenGeneration_SelectPartitionTests_Results

    [<Test>]
    let ``It should be possible to use an empty over clause`` () =
        let expr = Select.Column(<@ fun (p : Person) -> p.Name @>)
                         .Column(<@ fun (p : Person) -> Sql.Sum(p.Age) @>, Over.Empty)
                         .From<Person>()
        let result = expr.ToSqlExpression()

        result |> should equal emptyOverClauseExpression


    [<Test>]
    let ``It should be possible to partition by a column`` () =
      let expr = Select.Column(<@ fun (p : Person) -> p.Name @>)
                       .Column(<@ fun (p : Person) -> Sql.Max(p.Age) @>, Over.PartitionBy(<@ fun (p : Person) -> p.Name @>))
                       .From<Person>()
      let result = expr.ToSqlExpression()

      result |> should equal partitionByColumnExpression

    [<Test>]
    let ``It should be possible to partition by multiple columns`` () =
      let expr = Select.Column(<@ fun (p : Person) -> p.Name @>)
                       .Column(<@ fun (p : Person) -> Sql.Max(p.Age) @>, 
                               Over.PartitionBy(<@ fun (p : Person) -> p.Name @>)
                                   .ThenBy(<@ fun (p : Person) -> p.LivesAtHouseId @>))
                       .From<Person>()
      let result = expr.ToSqlExpression()

      result |> should equal partitionByMultipleColumnsExpression

    [<Test>]
    let ``It should be possible to order by a column`` () =
      let expr = Select.Column(<@ fun (p : Person) -> p.Name @>)
                       .Column(<@ fun (p : Person) -> Sql.Sum(p.Age) @>, 
                               Over.OrderBy(<@ fun (p : Person) -> p.Age @>))
                       .From<Person>()
      let result = expr.ToSqlExpression()
      result |> should equal orderByColumnExpression

    [<Test>]
    let ``It should be possible to order by multiple columns`` () =
      let expr = Select.Column(<@ fun (p : Person) -> p.Name@>)
                       .Column(<@ fun (p : Person) -> Sql.Sum(p.Age) @>, 
                               Over.OrderBy(<@ fun (p : Person) -> p.Age @>, Direction.Ascending, NullsOrdering.NullsLast)
                                   .ThenBy(<@ fun (p : Person) -> p.Name @>, Direction.Descending))
                       .From<Person>();
      let result = expr.ToSqlExpression()
      result |> should equal orderByMultipleColumnsExpression

    [<Test>]
    let ``It should be possible to both partition and order by a column`` () =
      let expr = Select.Column(<@ fun (p : Person) -> p.Name @>)
                       .Column(<@ fun (p : Person) -> Sql.Sum(p.Age) @>, 
                               Over.PartitionBy(<@ fun (p : Person) -> p.Name @>)
                                   .OrderBy(<@ fun (p : Person) -> p.Age @>))
                       .From<Person>();
      let result = expr.ToSqlExpression()
      result |> should equal orderAndPartitionByColumnExpression