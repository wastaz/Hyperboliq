namespace Hyperboliq.Migration.Tests

open NUnit
open NUnit.Framework
open FsUnit
open Hyperboliq
open Hyperboliq.Domain
open Hyperboliq.Domain.AST
open TestModel

module SelectTests =

    [<Test>]
    let ``It should be able to select all columns from a table`` () =
        let expr = Select.Star<Person>().From<Person>().ToSqlExpression()
        let expected =
            { Select = { IsDistinct = false; Values = [ ValueNode.StarColumn(StarColumnToken(Types.TableReferenceFromType<Person>)) ] }
              From = { Joins = []; Tables = [ TableIdentifier<Person>() ] }
              Where = None
              GroupBy = None
              OrderBy = None
            }
            |> PlainSelectExpression.Plain
            |> SelectExpression.Plain
            |> SqlExpression.Select
        expr |> should equal expected

