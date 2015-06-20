namespace Hyperboliq

open System
open System.Linq.Expressions
open Hyperboliq.Domain.Types
open Hyperboliq.Domain.Stream
open Hyperboliq.Domain.SqlGen
open Hyperboliq.Domain.ExpressionParts

type FluentDeleteBase(expr : DeleteExpression) =
    member x.Expression with internal get() = expr

    member x.ToSqlExpression () = (x :> ISqlExpressionTransformable).ToSqlExpression ()
    interface ISqlExpressionTransformable with
        member x.ToSqlExpression () = SqlExpression.Delete(expr)

    member x.ToSql (dialect : ISqlDialect) = (x :> ISqlStatement).ToSql(dialect)
    interface ISqlStatement with
        member x.ToSql(dialect : ISqlDialect) = x.ToSqlExpression() |> SqlifyExpression dialect

type DeleteWhere internal (expr : DeleteExpression) =
    inherit FluentDeleteBase(expr)
    let New expr = DeleteWhere(expr)

    member x.And<'a>(predicate : Expression<Func<'a, bool>>) =
        { expr with Where = Some(AddOrCreateWhereAndClause expr.Where predicate [| TableReferenceFromType<'a> |]) }
        |> New
    member x.And<'a, 'b>(predicate : Expression<Func<'a, 'b, bool>>) =
        { expr with Where = Some(AddOrCreateWhereAndClause expr.Where predicate [| TableReferenceFromType<'a>; TableReferenceFromType<'b> |]) }
        |> New

    member x.Or<'a>(predicate : Expression<Func<'a, bool>>) =
        { expr with Where = Some(AddOrCreateWhereOrClause expr.Where predicate [| TableReferenceFromType<'a> |]) }
        |> New
    member x.Or<'a, 'b>(predicate : Expression<Func<'a, 'b, bool>>) =
        { expr with Where = Some(AddOrCreateWhereOrClause expr.Where predicate [| TableReferenceFromType<'a>; TableReferenceFromType<'b> |]) }
        |> New


type DeleteFrom<'a> internal () =
    inherit FluentDeleteBase({ 
                                From = { Tables = [ TableIdentifier<'a>() ]; Joins = [] }
                                Where = None 
                             })
    
    member x.Where<'a>(predicate : Expression<Func<'a, bool>>) = DeleteWhere(x.Expression).And(predicate)
    member x.Where<'a, 'b>(predicate : Expression<Func<'a, bool>>) = DeleteWhere(x.Expression).And(predicate)


type Delete private () =
    static member From<'a>() = DeleteFrom<'a> ()
