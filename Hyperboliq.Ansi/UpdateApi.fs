namespace Hyperboliq

open System
open System.Linq.Expressions
open Hyperboliq
open Hyperboliq.Types
open Hyperboliq.Domain.AST
open Hyperboliq.Domain.ExpressionParts
open Hyperboliq.Domain.UpdateExpressionPart
open Hyperboliq.Domain.SqlGen
open Hyperboliq.Domain.ExpressionVisitor

type UpdateWhere<'a> internal (expr : UpdateExpression) =
    static let New expr = UpdateWhere<'a>(expr)

    member x.And(predicate : Expression<Func<'a, bool>>) =
        { expr with Where = Some(AddOrCreateWhereAndClause expr.Where (LinqExpression(predicate)) [| TableReferenceFromType<'a> |]) }
        |> New
    member x.Or(predicate : Expression<Func<'a, bool>>) =
        { expr with Where = Some(AddOrCreateWhereOrClause expr.Where (LinqExpression(predicate)) [| TableReferenceFromType<'a> |]) }
        |> New

    member x.ToSqlExpression () = (x :> ISqlExpressionTransformable).ToSqlExpression ()
    interface ISqlExpressionTransformable with
        member x.ToSqlExpression () = SqlExpression.Update(expr)

    member x.ToSql (dialect : ISqlDialect) = (x :> ISqlStatement).ToSql(dialect)
    interface ISqlStatement with
        member x.ToSql(dialect : ISqlDialect) = x.ToSqlExpression() |> SqlifyExpression dialect

type UpdateSet<'a> internal (expr : UpdateExpression) =
    static let New expr = UpdateSet<'a>(expr)

    new() = UpdateSet<'a>({ UpdateSet = { Table = TableReferenceFromType<'a>; SetExpressions = [] }; Where = None })

    member x.Set<'b>(selector : Expression<Func<'a, 'b>>, value : 'b) =
        { expr with UpdateSet = (AddObjectSetExpression expr.UpdateSet (LinqExpression(selector)) value) }
        |> New
    member x.Set<'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, value : 'b) =
        { expr with UpdateSet = (AddObjectSetExpression expr.UpdateSet (Quotation(selector)) value) }
        |> New

    member x.Set<'b>(selector : Expression<Func<'a, 'b>>, valueUpdate : Expression<Func<'a, 'b>>) =
        { expr with UpdateSet = (AddValueExpression expr.UpdateSet (LinqExpression(selector)) (LinqExpression(valueUpdate))) }
        |> New
    member x.Set<'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, valueUpdate : Quotations.Expr<'a -> 'b>) =
        { expr with UpdateSet = (AddValueExpression expr.UpdateSet (Quotation(selector)) (Quotation(valueUpdate))) }
        |> New

    member x.Set<'b>(selector : Expression<Func<'a, 'b>>, selectExpr : SelectExpression) = 
        { expr with UpdateSet = (AddSingleValueSetExpression expr.UpdateSet (LinqExpression(selector)) selectExpr) }
        |> New
    member x.Set<'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, selectExpr : SelectExpression) =
        { expr with UpdateSet = (AddSingleValueSetExpression expr.UpdateSet (Quotation(selector)) selectExpr) }
        |> New

    member x.Set<'b>(selector : Expression<Func<'a, 'b>>, selectExpr : ISelectExpressionTransformable) = x.Set(selector, selectExpr.ToSelectExpression())
    member x.Set<'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, selectExpr : ISelectExpressionTransformable) = x.Set(selector, selectExpr.ToSelectExpression())

    member x.Where(predicate : Expression<Func<'a, bool>>) = UpdateWhere(expr).And(predicate)

    member x.ToSqlExpression () = (x :> ISqlExpressionTransformable).ToSqlExpression ()
    interface ISqlExpressionTransformable with
        member x.ToSqlExpression () = SqlExpression.Update(expr)

    member x.ToSql (dialect : ISqlDialect) = (x :> ISqlStatement).ToSql(dialect)
    interface ISqlStatement with
        member x.ToSql(dialect : ISqlDialect) = x.ToSqlExpression() |> SqlifyExpression dialect

type Update<'a> private () =
    static member Set<'b>(selector : Expression<Func<'a, 'b>>, value : 'b) = UpdateSet<'a>().Set(selector, value)
    static member Set<'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, value : 'b) = UpdateSet<'a>().Set(selector, value);

    static member Set<'b>(selector : Expression<Func<'a, 'b>>, valueUpdate : Expression<Func<'a, 'b>>) = UpdateSet<'a>().Set(selector, valueUpdate)
    static member Set<'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, [<ReflectedDefinition>] valueUpdate : Quotations.Expr<'a -> 'b>) = UpdateSet<'a>().Set(selector, valueUpdate)

    static member Set<'b>(selector : Expression<Func<'a, 'b>>, selectExpr : SelectExpression) = UpdateSet<'a>().Set(selector, selectExpr)
    static member Set<'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, selectExpr : SelectExpression) = UpdateSet<'a>().Set(selector, selectExpr)

    static member Set<'b>(selector : Expression<Func<'a, 'b>>, selectExpr : ISelectExpressionTransformable) = UpdateSet<'a>().Set(selector, selectExpr)
    static member Set<'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, selectExpr : ISelectExpressionTransformable) = UpdateSet<'a>().Set(selector, selectExpr)
