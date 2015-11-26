namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_SetOperationTests =
  open NUnit.Framework
  open FsUnit
  open Hyperboliq
  open Hyperboliq.Domain.AST
  open Hyperboliq.Domain
  open Hyperboliq.Tests.TokenGeneration.TokenGeneration_SetOperationTests_Results

  [<Test>]
  let ``It can do a simple union`` () =
    let expr = 
      SetOperations.Union(
        Select.Star<Person>()
              .From<Person>()
              .Where(<@ fun (p : Person) -> p.Age > 42 @>),
        Select.Star<Person>()
              .From<Person>()
              .Where(<@ fun (p : Person) -> p.Name = "Kalle" @>));
    let result = expr.ToSqlExpression()
    result |> should equal simpleUnionExpression

  [<Test>]
  let ``It can do a simple union all`` () =
    let expr = 
      SetOperations.UnionAll(
        Select.Star<Person>()
              .From<Person>()
              .Where(<@ fun (p : Person) -> p.Age > 42 @>),
        Select.Star<Person>()
              .From<Person>()
              .Where(<@ fun (p : Person) -> p.Name = "Kalle" @>));
    let result = expr.ToSqlExpression()
    result |> should equal simpleUnionAllExpression

  [<Test>]
  let ``It can do a simple intersect`` () =
    let expr =
      SetOperations.Intersect(
        Select.Star<Person>()
              .From<Person>(),
        Select.Star<Person>()
              .From<Person>()
              .Where(<@ fun (p : Person) -> p.Age > 42 @>));
    let result = expr.ToSqlExpression()
    result |> should equal simpleIntersectExpression

  [<Test>]
  let ``It can do a simple minus`` () =
    let expr =
      SetOperations.Minus(
        Select.Star<Person>()
              .From<Person>(),
        Select.Star<Person>()
              .From<Person>()
              .Where(<@ fun (p : Person) -> p.Age > 42 @>));
    let result = expr.ToSqlExpression()
    result |> should equal simpleMinusExpression

  [<Test>]
  let ``It can do a union with common table expressions`` () =
    let identifier = Table<Person>.WithTableAlias("cte")
    let expr =
      With.Table(identifier,
                SetOperations.Union(
                  Select.Star<Person>()
                        .From<Person>()
                        .Where(<@ fun (p : Person) -> p.Age > 42 @>),
                  Select.Star<Person>()
                        .From<Person>()
                        .Where(<@ fun (p : Person) -> p.Name = "Kalle" @>)))
          .Query(
                SetOperations.Union(
                  Select.Star(identifier)
                        .From(identifier)
                        .Where(identifier, <@ fun (p : Person) -> p.Age = 50 @>),
                  Select.Star(identifier)
                        .From(identifier)
                        .Where(identifier, <@ fun (p : Person) -> p.Name = "Kalle" @>)))
    let result = expr.ToSqlExpression()
    result |> should equal unionsInACommonTableExpression

