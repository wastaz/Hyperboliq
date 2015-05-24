namespace Hyperboliq

open System
open System.Runtime.InteropServices
open System.Linq.Expressions
open Hyperboliq.Domain.Types
open Hyperboliq.Domain.Stream
open Hyperboliq.Domain.ExpressionParts
open Hyperboliq.Domain.SelectExpressionParts
open Hyperboliq.Domain.SqlGen

[<AbstractClass>]
type FluentSelectBase(expr : SelectExpression) =
    member x.Expression with internal get() = expr

    member x.ToSelectExpression () = (x :> ISelectExpressionTransformable).ToSelectExpression ()
    interface ISelectExpressionTransformable with
        member x.ToSelectExpression () = expr

    member x.ToSqlExpression () = (x :> ISqlExpressionTransformable).ToSqlExpression ()
    interface ISqlExpressionTransformable with
        member x.ToSqlExpression () = SqlExpression.Select(expr)

    member x.ToSql (dialect : ISqlDialect) = (x :> ISqlQuery).ToSql(dialect)
    interface ISqlQuery with
        member x.ToSql(dialect : ISqlDialect) = x.ToSqlExpression() |> SqlifyExpression dialect

type SelectOrderBy internal (expr : SelectExpression) =
    inherit FluentSelectBase(expr)
    static let New expr = SelectOrderBy(expr)

    member private x.InternalThenBy<'a>(selector : Expression<Func<'a, obj>>, ?direction : Direction, ?nullsOrdering : NullsOrdering) =
        let dir = defaultArg direction Direction.Ascending
        let nullsOrder = defaultArg nullsOrdering NullsOrdering.NullsUndefined
        { expr with OrderBy = Some(AddOrCreateOrderingClause expr.OrderBy TableReferenceFromType<'a> dir nullsOrder selector) }

    member x.ThenBy<'a>(selector : Expression<Func<'a, obj>>) = 
        x.InternalThenBy(selector) |> New
    member x.ThenBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction) = 
        x.InternalThenBy(selector, direction) |> New
    member x.ThenBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction, nullsOrdering : NullsOrdering) = 
        x.InternalThenBy(selector, direction, nullsOrdering) |> New

type SelectHaving internal  (expr : SelectExpression) =
    inherit FluentSelectBase(expr)
    static let New expr = SelectHaving(expr)

    member x.And<'a>(predicate : Expression<Func<'a, bool>>) = 
        { expr with GroupBy = Some(AddHavingAndClause expr.GroupBy predicate [| TableReferenceFromType<'a> |])}
        |> New

    member x.And<'a, 'b>(predicate : Expression<Func<'a, 'b, bool>>) =
        { expr with GroupBy = Some(AddHavingAndClause expr.GroupBy predicate [| TableReferenceFromType<'a>; TableReferenceFromType<'b> |])}
        |> New

    member x.Or<'a>(predicate : Expression<Func<'a, bool>>) =
        { expr with GroupBy = Some(AddHavingOrClause expr.GroupBy predicate [| TableReferenceFromType<'a> |]) }
        |> New

    member x.Or<'a, 'b>(predicate : Expression<Func<'a, 'b, bool>>) =
        { expr with GroupBy = Some(AddHavingOrClause expr.GroupBy predicate [| TableReferenceFromType<'a>; TableReferenceFromType<'b> |]) }
        |> New

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>) =
        SelectOrderBy(expr).ThenBy(selector)
    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction) =
        SelectOrderBy(expr).ThenBy(selector, direction)
    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction, nullsOrdering : NullsOrdering) =
        SelectOrderBy(expr).ThenBy(selector, direction, nullsOrdering)

type SelectGroupBy internal (expr : SelectExpression) =
    inherit FluentSelectBase(expr)
    static let New expr = SelectGroupBy(expr)

    member x.ThenBy<'a>(selector : Expression<Func<'a, obj>>) =
        { expr with GroupBy = Some(AddOrCreateGroupByClause expr.GroupBy selector [| TableReferenceFromType<'a> |])}
        |> New

    member x.Having<'a>(predicate : Expression<Func<'a, bool>>) = SelectHaving(expr).And(predicate)
    member x.Having<'a, 'b>(predicate : Expression<Func<'a, 'b, bool>>) = SelectHaving(expr).And(predicate)

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>) =
        SelectOrderBy(expr).ThenBy(selector)
    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction) =
        SelectOrderBy(expr).ThenBy(selector, direction)
    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction, nullsOrdering : NullsOrdering) =
        SelectOrderBy(expr).ThenBy(selector, direction, nullsOrdering)

type SelectWhere internal (expr : SelectExpression) =
    inherit FluentSelectBase(expr)
    static let New expr = SelectWhere(expr)

    member x.And<'a>(predicate : Expression<Func<'a, bool>>) =
        { expr with Where = AddOrCreateWhereAndClause expr.Where predicate [| TableReferenceFromType<'a> |] |> Some }
        |> New
    member x.And<'a, 'b>(predicate : Expression<Func<'a, 'b, bool>>) =
        { expr with Where = AddOrCreateWhereAndClause expr.Where predicate [| TableReferenceFromType<'a>; TableReferenceFromType<'b> |] |> Some }
        |> New
    member x.Or<'a>(predicate : Expression<Func<'a, bool>>) =
        { expr with Where = AddOrCreateWhereOrClause expr.Where predicate [| TableReferenceFromType<'a> |] |> Some }
        |> New
    member x.Or<'a, 'b>(predicate : Expression<Func<'a, 'b, bool>>) =
        { expr with Where = AddOrCreateWhereOrClause expr.Where predicate [| TableReferenceFromType<'a>; TableReferenceFromType<'b> |] |> Some }
        |> New

    member x.GroupBy<'a>(selector : Expression<Func<'a, obj>>) =
        SelectGroupBy(expr).ThenBy(selector)

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>) =
        SelectOrderBy(expr).ThenBy(selector)
    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction) =
        SelectOrderBy(expr).ThenBy(selector, direction)
    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction, nullsOrdering : NullsOrdering) =
        SelectOrderBy(expr).ThenBy(selector, direction, nullsOrdering)

type Join internal (expr : SelectExpression) =
    inherit FluentSelectBase(expr)
    static let New ex = Join(ex)
    
    let JoinWithArray joinType src tgt predicate =
        { expr with From = { expr.From with Joins = (CreateJoinClause joinType predicate tgt src) :: expr.From.Joins } }
    let Join2 joinType src tgt predicate = JoinWithArray joinType [| src |] tgt predicate
    let Join3 joinType src1 src2 tgt predicate = JoinWithArray joinType [| src1; src2 |] tgt predicate

    member x.InnerJoin<'src, 'tgt>(predicate : Expression<Func<'src, 'tgt, bool>>) =
        Join2 JoinType.InnerJoin TableReferenceFromType<'src> TableReferenceFromType<'tgt> predicate
        |> New
    member x.InnerJoin<'src, 'tgt>(source : ITableReference<'src>, target : ITableReference<'tgt>, predicate : Expression<Func<'src, 'tgt, bool>>) =
        Join2 JoinType.InnerJoin source target predicate
        |> New
    member x.InnerJoin<'src1, 'src2, 'tgt>(predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) =
        Join3 JoinType.InnerJoin TableReferenceFromType<'src1> TableReferenceFromType<'src2> TableReferenceFromType<'tgt> predicate
        |> New
    member x.InnerJoin<'src1, 'src2, 'tgt>(source1 : ITableReference<'src1>, source2 : ITableReference<'src2>, target : ITableReference<'tgt>, predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) =
        Join3 JoinType.InnerJoin source1 source2 target predicate
        |> New

    member x.LeftJoin<'src, 'tgt>(predicate : Expression<Func<'src, 'tgt, bool>>) =
        Join2 JoinType.LeftJoin TableReferenceFromType<'src> TableReferenceFromType<'tgt> predicate
        |> New
    member x.LeftJoin<'src1, 'src2, 'tgt>(predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) =
        Join3 JoinType.LeftJoin TableReferenceFromType<'src1> TableReferenceFromType<'src2> TableReferenceFromType<'tgt> predicate
        |> New

    member x.RightJoin<'src, 'tgt>(predicate : Expression<Func<'src, 'tgt, bool>>) =
        Join2 JoinType.RightJoin TableReferenceFromType<'src> TableReferenceFromType<'tgt> predicate
        |> New
    member x.RightJoin<'src1, 'src2, 'tgt>(predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) =
        Join3 JoinType.RightJoin TableReferenceFromType<'src1> TableReferenceFromType<'src2> TableReferenceFromType<'tgt> predicate
        |> New

    member x.FullJoin<'src, 'tgt>(predicate : Expression<Func<'src, 'tgt, bool>>) =
        Join2 JoinType.FullJoin TableReferenceFromType<'src> TableReferenceFromType<'tgt> predicate
        |> New
    member x.FullJoin<'src1, 'src2, 'tgt>(predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) =
        Join3 JoinType.FullJoin TableReferenceFromType<'src1> TableReferenceFromType<'src2> TableReferenceFromType<'tgt> predicate
        |> New

    member x.Where<'a>(predicate : Expression<Func<'a, bool>>) = SelectWhere(expr).And(predicate)
    member x.Where<'a, 'b>(predicate : Expression<Func<'a, 'b, bool>>) = SelectWhere(expr).And(predicate)

    member x.GroupBy<'a>(selector : Expression<Func<'a, obj>>) =
        SelectGroupBy(expr).ThenBy(selector)

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>) =
        SelectOrderBy(expr).ThenBy(selector)
    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction) =
        SelectOrderBy(expr).ThenBy(selector, direction)
    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction, nullsOrdering : NullsOrdering) =
        SelectOrderBy(expr).ThenBy(selector, direction, nullsOrdering)

type SelectFrom<'a> internal (tbl: ITableReference, exprNode : SelectExpressionNode) =
    inherit FluentSelectBase({
                                 Select = exprNode
                                 From = AddFromTable <| NewFromExpression () <| tbl
                                 Where = None
                                 GroupBy = None
                                 OrderBy = None
                             })

    member x.InnerJoin<'src, 'tgt>(predicate : Expression<Func<'src, 'tgt, bool>>) = Join(x.Expression).InnerJoin(predicate)
    member x.InnerJoin<'src1, 'src2, 'tgt>(predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) = Join(x.Expression).InnerJoin(predicate)
    member x.InnerJoin<'src, 'tgt>(source : ITableReference<'src>, target : ITableReference<'tgt>, predicate : Expression<Func<'src, 'tgt, bool>>) =
        Join(x.Expression).InnerJoin(source, target, predicate)
    member x.InnerJoin<'src1, 'src2, 'tgt>(source1 : ITableReference<'src1>, source2 : ITableReference<'src2>, target : ITableReference<'tgt>, predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) =
        Join(x.Expression).InnerJoin(source1, source2, target, predicate)

    member x.LeftJoin<'src, 'tgt>(predicate : Expression<Func<'src, 'tgt, bool>>) = Join(x.Expression).LeftJoin(predicate)
    member x.LeftJoin<'src1, 'src2, 'tgt>(predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) = Join(x.Expression).LeftJoin(predicate)
    
    member x.RightJoin<'src, 'tgt>(predicate : Expression<Func<'src, 'tgt, bool>>) = Join(x.Expression).RightJoin(predicate)
    member x.RightJoin<'src1, 'src2, 'tgt>(predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) = Join(x.Expression).RightJoin(predicate)

    member x.FullJoin<'src, 'tgt>(predicate : Expression<Func<'src, 'tgt, bool>>) = Join(x.Expression).FullJoin(predicate)
    member x.FullJoin<'src1, 'src2, 'tgt>(predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) = Join(x.Expression).FullJoin(predicate)

    member x.Where<'a>(predicate : Expression<Func<'a, bool>>) = SelectWhere(x.Expression).And predicate
    member x.Where<'a, 'b>(predicate : Expression<Func<'a, 'b, bool>>) = SelectWhere(x.Expression).And predicate

    member x.GroupBy<'a>(selector : Expression<Func<'a, obj>>) =
        SelectGroupBy(x.Expression).ThenBy(selector)

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>) =
        SelectOrderBy(x.Expression).ThenBy(selector)
    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction) =
        SelectOrderBy(x.Expression).ThenBy(selector, direction)
    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction, nullsOrdering : NullsOrdering) =
        SelectOrderBy(x.Expression).ThenBy(selector, direction, nullsOrdering)

type SelectImpl internal (expr : SelectExpressionNode) =
    internal new () = SelectImpl(NewSelectExpression ())

    member x.Distinct 
        with get() = SelectImpl(MakeDistinct expr)
    member x.Star<'a>() = 
        SelectImpl(SelectAllColumns expr TableReferenceFromType<'a>)
    member x.Column<'a>(selector : Expression<Func<'a, obj>>) = 
        SelectImpl(SelectColumns expr selector TableReferenceFromType<'a>)
    member x.Column<'a>(tbl : ITableReference<'a>, selector : Expression<Func<'a, obj>>) = 
        SelectImpl(SelectColumns expr selector tbl)
    member x.From<'a>(tbl : ITableReference<'a>) = SelectFrom<'a>(tbl, expr)
    member x.From<'a>() = x.From<'a>(TableReferenceFromType<'a>)

type Select private () =
    static member Distinct with get() = (SelectImpl()).Distinct
    static member Star<'a>() = (SelectImpl()).Star<'a>()
    static member Column<'a>(selector : Expression<Func<'a, obj>>) = (SelectImpl()).Column<'a>(selector)
    static member Column<'a>(tbl : ITableReference<'a>, selector : Expression<Func<'a, obj>>) = (SelectImpl()).Column<'a>(tbl, selector)
