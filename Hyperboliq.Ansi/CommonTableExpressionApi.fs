namespace Hyperboliq

open System
open System.Linq.Expressions
open Hyperboliq.Domain.Types
open Hyperboliq.Domain.Stream
open Hyperboliq.Domain.SqlGen

type WithSelect internal (expr : SelectExpression) =
    member x.ToSelectExpression () = (x :> ISelectExpressionTransformable).ToSelectExpression ()
    interface ISelectExpressionTransformable with
        member x.ToSelectExpression () = expr

    member x.ToSqlExpression () = (x :> ISqlExpressionTransformable).ToSqlExpression ()
    interface ISqlExpressionTransformable with
        member x.ToSqlExpression () = SqlExpression.Select(expr)

    member x.ToSql (dialect : ISqlDialect) = (x :> ISqlQuery).ToSql(dialect)
    interface ISqlQuery with
        member x.ToSql(dialect : ISqlDialect) = x.ToSqlExpression() |> SqlifyExpression dialect


type WithImpl internal (ctdList : ICommonTableDefinition list) =
    static let New lst = WithImpl(lst)

    member x.Table<'a>(ref : ITableReference<'a>, query : ISqlQuery) = { Query = query; TableReference = ref } :> ICommonTableDefinition :: ctdList |> New
    member x.Table<'a>(query : ISqlQuery) = x.Table<'a>(TableReferenceFromType<'a>, query)

    member x.Query(statement : ISqlExpressionTransformable) = 
        match statement.ToSqlExpression() with
        | SqlExpression.Select(s) -> WithSelect({ s with With = { Definitions = ctdList } |> Some })
        | _ -> failwith "Not implemented"


type With private () =
    static member Table<'a>(query : ISqlQuery) = WithImpl([]).Table<'a>(query)
    static member Table<'a>(ref : ITableReference<'a>, query : ISqlQuery) = WithImpl([]).Table<'a>(ref, query)
