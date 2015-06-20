namespace Hyperboliq

open System
open System.Linq.Expressions
open Hyperboliq.Domain.Types
open Hyperboliq.Domain.Stream
open Hyperboliq.Domain.SqlGen

type WithSelect internal (expr : CommonTableValuedSelectExpression) =
    member x.ToSelectExpression () = (x :> ISelectExpressionTransformable).ToSelectExpression ()
    interface ISelectExpressionTransformable with
        member x.ToSelectExpression () = Complex(expr)

    member x.ToSqlExpression () = (x :> ISqlExpressionTransformable).ToSqlExpression ()
    interface ISqlExpressionTransformable with
        member x.ToSqlExpression () = SqlExpression.Select(Complex(expr))

    member x.ToSql (dialect : ISqlDialect) = (x :> ISqlQuery).ToSql(dialect)
    interface ISqlQuery with
        member x.ToSql(dialect : ISqlDialect) = x.ToSqlExpression() |> SqlifyExpression dialect


type WithImpl internal (ctdList : ICommonTableDefinition list) =
    static let New lst = WithImpl(lst)

    member x.Table<'a>(def : ITableIdentifier<'a>, query : ISelectExpressionTransformable) = 
        match query.ToSelectExpression() with
        | Plain(q) -> { Query = q; TableDef = def } :> ICommonTableDefinition :: ctdList |> New
        | _ -> failwith "Not implemented"
    member x.Table<'a>(query : ISelectExpressionTransformable) = x.Table<'a>((TableIdentifier<'a>()), query)

    member x.Query(statement : ISqlExpressionTransformable) = 
        match statement.ToSqlExpression() with
        | SqlExpression.Select(Plain(s)) -> WithSelect({ Definitions = ctdList }, s)
        | _ -> failwith "Not implemented"


type With private () =
    static member Table<'a>(query : ISelectExpressionTransformable) = WithImpl([]).Table<'a>(query)
    static member Table<'a>(ref : ITableIdentifier<'a>, query : ISelectExpressionTransformable) = WithImpl([]).Table<'a>(ref, query)
