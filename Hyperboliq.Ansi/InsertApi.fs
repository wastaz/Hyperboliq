namespace Hyperboliq

open System
open System.Linq.Expressions
open Hyperboliq
open Hyperboliq.Types
open Hyperboliq.Domain.Stream
open Hyperboliq.Domain.InsertExpressionPart
open Hyperboliq.Domain.SqlGen

type InsertValues<'a> internal (expr : InsertExpression) =
    let HandleValue state instance =
        { state with InsertValues = (AddInsertValue state.InsertInto state.InsertValues instance) }

    member x.Value(instance : 'a) = 
        InsertValues<'a>(HandleValue expr instance)

    member x.Values([<System.ParamArray>] instances : 'a array) = 
        InsertValues<'a>(Array.fold HandleValue expr instances)

    member x.ToSqlExpression () = (x :> ISqlExpressionTransformable).ToSqlExpression ()
    interface ISqlExpressionTransformable with
        member x.ToSqlExpression () = SqlExpression.Insert(expr)

    member x.ToSql (dialect : ISqlDialect) = (x :> ISqlStatement).ToSql(dialect)
    interface ISqlStatement with
        member x.ToSql(dialect : ISqlDialect) = x.ToSqlExpression() |> SqlifyExpression dialect

type InsertInto<'a> internal () =
    let expr = { InsertInto = { Table = TableReferenceFromType<'a>; Columns = [] }; InsertValues = [] }

    member x.AllColumns 
        with get() = InsertValues<'a>({ expr with InsertInto = (AddAllColumns expr.InsertInto) })

    member x.Columns(selector : Expression<Func<'a, obj>>) = 
        InsertValues<'a>({ expr with InsertInto = (AddColumns expr.InsertInto selector) })

type Insert private () =
    static member Into<'a>() = InsertInto<'a>()

