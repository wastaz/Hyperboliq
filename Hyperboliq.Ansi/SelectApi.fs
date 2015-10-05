namespace Hyperboliq

open System
open System.Runtime.InteropServices
open System.Linq.Expressions
open Hyperboliq
open Hyperboliq.Types
open Hyperboliq.Domain.AST
open Hyperboliq.Domain.ExpressionParts
open Hyperboliq.Domain.SqlGen
open Hyperboliq.Domain.ExpressionVisitor

[<AbstractClass>]
type FluentSelectBase(expr : SelectExpressionToken) =
    member x.Expression with internal get() = expr

    member x.ToPlainSelectExpression () = (x :> IPlainSelectExpressionTransformable).ToPlainSelectExpression ()
    interface IPlainSelectExpressionTransformable with
        member x.ToPlainSelectExpression () = PlainSelectExpression.Plain(x.Expression)

    member x.ToSelectExpression () = (x :> ISelectExpressionTransformable).ToSelectExpression ()
    interface ISelectExpressionTransformable with
        member x.ToSelectExpression () = SelectExpression.Plain(x.ToPlainSelectExpression ())

    member x.ToSqlExpression () = (x :> ISqlExpressionTransformable).ToSqlExpression ()
    interface ISqlExpressionTransformable with
        member x.ToSqlExpression () = SqlExpression.Select(x.ToSelectExpression ())

    member x.ToSql (dialect : ISqlDialect) = (x :> ISqlQuery).ToSql(dialect)
    interface ISqlQuery with
        member x.ToSql(dialect : ISqlDialect) = x.ToSqlExpression() |> SqlifyExpression dialect

type SelectOrderBy internal (expr : SelectExpressionToken) =
    inherit FluentSelectBase(expr)
    static let New expr = SelectOrderBy(expr)

    member private x.InternalThenBy<'a>(selector : VisitableExpression, ?direction : Direction, ?nullsOrdering : NullsOrdering) =
        let dir = defaultArg direction Direction.Ascending
        let nullsOrder = defaultArg nullsOrdering NullsOrdering.NullsUndefined
        { expr with OrderBy = Some(AddOrCreateOrderingClause expr.OrderBy TableReferenceFromType<'a> dir nullsOrder selector) }

    member x.ThenBy<'a>(selector : Expression<Func<'a, obj>>) = x.InternalThenBy<'a>(LinqExpression(selector)) |> New
    member x.ThenBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>) = x.InternalThenBy<'a>(Quotation(selector)) |> New 

    member x.ThenBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction) = x.InternalThenBy<'a>(LinqExpression(selector), direction) |> New
    member x.ThenBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, direction : Direction) = x.InternalThenBy<'a>(Quotation(selector), direction) |> New

    member x.ThenBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction, nullsOrdering : NullsOrdering) = x.InternalThenBy<'a>(LinqExpression(selector), direction, nullsOrdering) |> New
    member x.ThenBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, direction : Direction, nullsOrdering : NullsOrdering) = x.InternalThenBy<'a>(Quotation(selector), direction, nullsOrdering) |> New

type SelectHaving internal  (expr : SelectExpressionToken) =
    inherit FluentSelectBase(expr)
    static let New expr = SelectHaving(expr)

    member x.And<'a>(predicate : Expression<Func<'a, bool>>) = 
        { expr with GroupBy = Some(AddHavingAndClause expr.GroupBy (LinqExpression(predicate)) [| TableReferenceFromType<'a> |])}
        |> New
    member x.And<'a>([<ReflectedDefinition>] predicate : Quotations.Expr<'a -> bool>) =
        { expr with GroupBy = Some(AddHavingAndClause expr.GroupBy (Quotation(predicate)) [| TableReferenceFromType<'a> |])}
        |> New

    member x.And<'a, 'b>(predicate : Expression<Func<'a, 'b, bool>>) =
        { expr with GroupBy = Some(AddHavingAndClause expr.GroupBy (LinqExpression(predicate)) [| TableReferenceFromType<'a>; TableReferenceFromType<'b> |])}
        |> New
    member x.And<'a, 'b>([<ReflectedDefinition>] predicate : Quotations.Expr<'a -> 'b -> bool>) =
        { expr with GroupBy = Some(AddHavingAndClause expr.GroupBy (Quotation(predicate)) [| TableReferenceFromType<'a>; TableReferenceFromType<'b> |])}
        |> New

    member x.Or<'a>(predicate : Expression<Func<'a, bool>>) =
        { expr with GroupBy = Some(AddHavingOrClause expr.GroupBy (LinqExpression(predicate)) [| TableReferenceFromType<'a> |]) }
        |> New
    member x.Or<'a>([<ReflectedDefinition>] predicate : Quotations.Expr<'a -> bool>) =
        { expr with GroupBy = Some(AddHavingOrClause expr.GroupBy (Quotation(predicate)) [| TableReferenceFromType<'a> |]) }
        |> New

    member x.Or<'a, 'b>(predicate : Expression<Func<'a, 'b, bool>>) =
        { expr with GroupBy = Some(AddHavingOrClause expr.GroupBy (LinqExpression(predicate)) [| TableReferenceFromType<'a>; TableReferenceFromType<'b> |]) }
        |> New
    member x.Or<'a, 'b>([<ReflectedDefinition>] predicate : Quotations.Expr<'a -> 'b -> bool>) =
        { expr with GroupBy = Some(AddHavingOrClause expr.GroupBy (Quotation(predicate)) [| TableReferenceFromType<'a>; TableReferenceFromType<'b> |]) }
        |> New

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>) = SelectOrderBy(expr).ThenBy(selector)
    member x.OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>) = SelectOrderBy(expr).ThenBy(selector)

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction) = SelectOrderBy(expr).ThenBy(selector, direction)
    member x.OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, direction : Direction) = SelectOrderBy(expr).ThenBy(selector, direction)

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction, nullsOrdering : NullsOrdering) = SelectOrderBy(expr).ThenBy(selector, direction, nullsOrdering)
    member x.OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, direction : Direction, nullsOrdering : NullsOrdering) = SelectOrderBy(expr).ThenBy(selector, direction, nullsOrdering)

type SelectGroupBy internal (expr : SelectExpressionToken) =
    inherit FluentSelectBase(expr)
    static let New expr = SelectGroupBy(expr)

    member x.ThenBy<'a>(selector : Expression<Func<'a, obj>>) =
        { expr with GroupBy = Some(AddOrCreateGroupByClause expr.GroupBy (LinqExpression(selector)) [| TableReferenceFromType<'a> |])}
        |> New
    member x.ThenBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>) =
        { expr with GroupBy = Some(AddOrCreateGroupByClause expr.GroupBy (Quotation(selector)) [| TableReferenceFromType<'a> |])}
        |> New

    member x.Having<'a>(predicate : Expression<Func<'a, bool>>) = SelectHaving(expr).And(predicate)
    member x.Having<'a>([<ReflectedDefinition>] predicate : Quotations.Expr<'a -> bool>) = SelectHaving(expr).And(predicate)

    member x.Having<'a, 'b>(predicate : Expression<Func<'a, 'b, bool>>) = SelectHaving(expr).And(predicate)

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>) = SelectOrderBy(expr).ThenBy(selector)
    member x.OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>) = SelectOrderBy(expr).ThenBy(selector)

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction) = SelectOrderBy(expr).ThenBy(selector, direction)
    member x.OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, direction : Direction) = SelectOrderBy(expr).ThenBy(selector, direction)

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction, nullsOrdering : NullsOrdering) = SelectOrderBy(expr).ThenBy(selector, direction, nullsOrdering)
    member x.OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, direction : Direction, nullsOrdering : NullsOrdering) = SelectOrderBy(expr).ThenBy(selector, direction, nullsOrdering)

type SelectWhere internal (expr : SelectExpressionToken) =
    inherit FluentSelectBase(expr)
    static let New expr = SelectWhere(expr)

    member x.And<'a>(predicate : Expression<Func<'a, bool>>) =
        { expr with Where = AddOrCreateWhereAndClause expr.Where (LinqExpression(predicate)) [| TableReferenceFromType<'a> |] |> Some }
        |> New
    member x.And<'a>([<ReflectedDefinition>] predicate : Quotations.Expr<'a -> bool>) =
        { expr with Where = AddOrCreateWhereAndClause expr.Where (Quotation(predicate)) [| TableReferenceFromType<'a>|] |> Some }
        |> New

    member x.And<'a, 'b>(predicate : Expression<Func<'a, 'b, bool>>) =
        { expr with Where = AddOrCreateWhereAndClause expr.Where (LinqExpression(predicate)) [| TableReferenceFromType<'a>; TableReferenceFromType<'b> |] |> Some }
        |> New
    member x.And<'a, 'b>([<ReflectedDefinition>] predicate : Quotations.Expr<'a -> 'b -> bool>) =
        { expr with Where = AddOrCreateWhereAndClause expr.Where (Quotation(predicate)) [| TableReferenceFromType<'a>; TableReferenceFromType<'b> |] |> Some }
        |> New

    member x.Or<'a>(predicate : Expression<Func<'a, bool>>) =
        { expr with Where = AddOrCreateWhereOrClause expr.Where (LinqExpression(predicate)) [| TableReferenceFromType<'a> |] |> Some }
        |> New
    member x.Or<'a>([<ReflectedDefinition>] predicate : Quotations.Expr<'a -> bool>) =
        { expr with Where = AddOrCreateWhereOrClause expr.Where (Quotation(predicate)) [| TableReferenceFromType<'a> |] |> Some }
        |> New

    member x.Or<'a, 'b>(predicate : Expression<Func<'a, 'b, bool>>) =
        { expr with Where = AddOrCreateWhereOrClause expr.Where (LinqExpression(predicate)) [| TableReferenceFromType<'a>; TableReferenceFromType<'b> |] |> Some }
        |> New
    member x.Or<'a, 'b>([<ReflectedDefinition>] predicate : Quotations.Expr<'a -> 'b -> bool>) =
        { expr with Where = AddOrCreateWhereOrClause expr.Where (Quotation(predicate)) [| TableReferenceFromType<'a>; TableReferenceFromType<'b> |] |> Some }
        |> New

    member x.GroupBy<'a>(selector : Expression<Func<'a, obj>>) = SelectGroupBy(expr).ThenBy(selector)
    member x.GroupBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>) = SelectGroupBy(expr).ThenBy(selector)

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>) = SelectOrderBy(expr).ThenBy(selector)
    member x.OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>) = SelectOrderBy(expr).ThenBy(selector)
    
    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction) = SelectOrderBy(expr).ThenBy(selector, direction)
    member x.OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, direction : Direction) = SelectOrderBy(expr).ThenBy(selector, direction)

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction, nullsOrdering : NullsOrdering) = SelectOrderBy(expr).ThenBy(selector, direction, nullsOrdering)
    member x.OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, direction : Direction, nullsOrdering : NullsOrdering) = SelectOrderBy(expr).ThenBy(selector, direction, nullsOrdering)

type Join internal (expr : SelectExpressionToken) =
    inherit FluentSelectBase(expr)
    static let New ex = Join(ex)

    let JoinWithArray joinType src tgt predicate =
        { expr with From = { expr.From with Joins = (CreateJoinClause joinType predicate tgt src) :: expr.From.Joins } }
    let Join2 joinType src tgt predicate = JoinWithArray joinType [| src |] tgt predicate
    let Join3 joinType src1 src2 tgt predicate = JoinWithArray joinType [| src1; src2 |] tgt predicate

    member x.InnerJoin<'src, 'tgt>(predicate : Expression<Func<'src, 'tgt, bool>>) =
        Join2 JoinType.InnerJoin (TableIdentifier<'src>()) (TableIdentifier<'tgt>()) (LinqExpression(predicate))
        |> New
    member x.InnerJoin<'src, 'tgt>([<ReflectedDefinition>] predicate : Quotations.Expr<'src -> 'tgt -> bool>) =
        Join2 JoinType.InnerJoin (TableIdentifier<'src>()) (TableIdentifier<'tgt>()) (Quotation(predicate))
        |> New

    member x.InnerJoin<'src, 'tgt>(source : ITableIdentifier<'src>, target : ITableIdentifier<'tgt>, predicate : Expression<Func<'src, 'tgt, bool>>) =
        Join2 JoinType.InnerJoin source target (LinqExpression(predicate))
        |> New
    member x.InnerJoin<'src, 'tgt>(source : ITableIdentifier<'src>, target : ITableIdentifier<'tgt>, [<ReflectedDefinition>] predicate : Quotations.Expr<'src -> 'tgt -> bool>) =
        Join2 JoinType.InnerJoin source target (Quotation(predicate))
        |> New

    member x.InnerJoin<'src1, 'src2, 'tgt>(predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) =
        Join3 JoinType.InnerJoin (TableIdentifier<'src1>()) (TableIdentifier<'src2>()) (TableIdentifier<'tgt>()) (LinqExpression(predicate))
        |> New
    member x.InnerJoin<'src1, 'src2, 'tgt>([<ReflectedDefinition>] predicate : Quotations.Expr<'src1 -> 'src2 -> 'tgt -> bool>) =
        Join3 JoinType.InnerJoin (TableIdentifier<'src1>()) (TableIdentifier<'src2>()) (TableIdentifier<'tgt>()) (Quotation(predicate))
        |> New
    
    member x.InnerJoin<'src1, 'src2, 'tgt>(source1 : ITableIdentifier<'src1>, source2 : ITableIdentifier<'src2>, target : ITableIdentifier<'tgt>, predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) =
        Join3 JoinType.InnerJoin source1 source2 target (LinqExpression(predicate))
        |> New
    member x.InnerJoin<'src1, 'src2, 'tgt>(source1 : ITableIdentifier<'src1>, source2 : ITableIdentifier<'src2>, target : ITableIdentifier<'tgt>, [<ReflectedDefinition>] predicate : Quotations.Expr<'src1 -> 'src2 -> 'tgt -> bool>) =
        Join3 JoinType.InnerJoin source1 source2 target (Quotation(predicate))
        |> New

    member x.LeftJoin<'src, 'tgt>(predicate : Expression<Func<'src, 'tgt, bool>>) =
        Join2 JoinType.LeftJoin (TableIdentifier<'src>()) (TableIdentifier<'tgt>()) (LinqExpression(predicate))
        |> New
    member x.LeftJoin<'src, 'tgt>([<ReflectedDefinition>] predicate : Quotations.Expr<'src -> 'tgt -> bool>) =
        Join2 JoinType.LeftJoin (TableIdentifier<'src>()) (TableIdentifier<'tgt>()) (Quotation(predicate))
        |> New

    member x.LeftJoin<'src1, 'src2, 'tgt>(predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) =
        Join3 JoinType.LeftJoin (TableIdentifier<'src1>()) (TableIdentifier<'src2>()) (TableIdentifier<'tgt>()) (LinqExpression(predicate))
        |> New
    member x.LeftJoin<'src1, 'src2, 'tgt>([<ReflectedDefinition>] predicate : Quotations.Expr<'src1 -> 'src2 -> 'tgt -> bool>) =
        Join3 JoinType.LeftJoin (TableIdentifier<'src1>()) (TableIdentifier<'src2>()) (TableIdentifier<'tgt>()) (Quotation(predicate))
        |> New

    member x.RightJoin<'src, 'tgt>(predicate : Expression<Func<'src, 'tgt, bool>>) =
        Join2 JoinType.RightJoin (TableIdentifier<'src>()) (TableIdentifier<'tgt>()) (LinqExpression(predicate))
        |> New
    member x.RightJoin<'src, 'tgt>([<ReflectedDefinition>] predicate : Quotations.Expr<'src -> 'tgt -> bool>) =
        Join2 JoinType.RightJoin (TableIdentifier<'src>()) (TableIdentifier<'tgt>()) (Quotation(predicate))
        |> New
    
    member x.RightJoin<'src1, 'src2, 'tgt>(predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) =
        Join3 JoinType.RightJoin (TableIdentifier<'src1>()) (TableIdentifier<'src2>()) (TableIdentifier<'tgt>()) (LinqExpression(predicate))
        |> New
    member x.RightJoin<'src1, 'src2, 'tgt>([<ReflectedDefinition>] predicate : Quotations.Expr<'src1 -> 'src2 -> 'tgt -> bool>) =
        Join3 JoinType.RightJoin (TableIdentifier<'src1>()) (TableIdentifier<'src2>()) (TableIdentifier<'tgt>()) (Quotation(predicate))
        |> New

    member x.FullJoin<'src, 'tgt>(predicate : Expression<Func<'src, 'tgt, bool>>) =
        Join2 JoinType.FullJoin (TableIdentifier<'src>()) (TableIdentifier<'tgt>()) (LinqExpression(predicate))
        |> New
    member x.FullJoin<'src, 'tgt>([<ReflectedDefinition>] predicate : Quotations.Expr<'src -> 'tgt -> bool>) =
        Join2 JoinType.FullJoin (TableIdentifier<'src>()) (TableIdentifier<'tgt>()) (Quotation(predicate))
        |> New

    member x.FullJoin<'src1, 'src2, 'tgt>(predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) =
        Join3 JoinType.FullJoin (TableIdentifier<'src1>()) (TableIdentifier<'src2>()) (TableIdentifier<'tgt>()) (LinqExpression(predicate))
        |> New
    member x.FullJoin<'src1, 'src2, 'tgt>([<ReflectedDefinition>] predicate : Quotations.Expr<'src1 -> 'src2 -> 'tgt -> bool>) =
        Join3 JoinType.FullJoin (TableIdentifier<'src1>()) (TableIdentifier<'src2>()) (TableIdentifier<'tgt>()) (Quotation(predicate))
        |> New

    member x.Where<'a>(predicate : Expression<Func<'a, bool>>) = SelectWhere(expr).And(predicate)
    member x.Where<'a>([<ReflectedDefinition>] predicate : Quotations.Expr<'a -> bool>) = SelectWhere(expr).And(predicate)

    member x.Where<'a, 'b>(predicate : Expression<Func<'a, 'b, bool>>) = SelectWhere(expr).And(predicate)
    member x.Where<'a, 'b>([<ReflectedDefinition>] predicate : Quotations.Expr<'a -> 'b -> bool>) = SelectWhere(expr).And(predicate)

    member x.GroupBy<'a>(selector : Expression<Func<'a, obj>>) = SelectGroupBy(expr).ThenBy(selector)
    member x.GroupBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>) = SelectGroupBy(expr).ThenBy(selector)

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>) = SelectOrderBy(expr).ThenBy(selector)
    member x.OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>) = SelectOrderBy(expr).ThenBy(selector)

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction) = SelectOrderBy(expr).ThenBy(selector, direction)
    member x.OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, direction : Direction) = SelectOrderBy(expr).ThenBy(selector, direction)

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction, nullsOrdering : NullsOrdering) = SelectOrderBy(expr).ThenBy(selector, direction, nullsOrdering)
    member x.OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, direction : Direction, nullsOrdering : NullsOrdering) = SelectOrderBy(expr).ThenBy(selector, direction, nullsOrdering)

type SelectFrom<'a> internal (tbl: ITableIdentifier, exprNode : SelectValuesExpressionNode) =
    inherit FluentSelectBase({
                                 Select = exprNode
                                 From = { Tables = [ tbl ]; Joins = [] }
                                 Where = None
                                 GroupBy = None
                                 OrderBy = None
                             })

    member x.InnerJoin<'src, 'tgt>(predicate : Expression<Func<'src, 'tgt, bool>>) = Join(x.Expression).InnerJoin(predicate)
    member x.InnerJoin<'src, 'tgt>([<ReflectedDefinition>] predicate : Quotations.Expr<'src -> 'tgt -> bool>) = Join(x.Expression).InnerJoin(predicate)

    member x.InnerJoin<'src1, 'src2, 'tgt>(predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) = Join(x.Expression).InnerJoin(predicate)
    member x.InnerJoin<'src1, 'src2, 'tgt>([<ReflectedDefinition>] predicate : Quotations.Expr<'src1 -> 'src2 -> 'tgt -> bool>) = Join(x.Expression).InnerJoin(predicate)

    member x.InnerJoin<'src, 'tgt>(source : ITableIdentifier<'src>, target : ITableIdentifier<'tgt>, predicate : Expression<Func<'src, 'tgt, bool>>) =
        Join(x.Expression).InnerJoin(source, target, predicate)
    member x.InnerJoin<'src, 'tgt>(source : ITableIdentifier<'src>, target : ITableIdentifier<'tgt>, [<ReflectedDefinition>] predicate : Quotations.Expr<'src -> 'tgt -> bool>) =
        Join(x.Expression).InnerJoin(source, target, predicate)

    member x.InnerJoin<'src1, 'src2, 'tgt>(source1 : ITableIdentifier<'src1>, source2 : ITableIdentifier<'src2>, target : ITableIdentifier<'tgt>, predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) =
        Join(x.Expression).InnerJoin(source1, source2, target, predicate)
    member x.InnerJoin<'src1, 'src2, 'tgt>(source1 : ITableIdentifier<'src1>, source2 : ITableIdentifier<'src2>, target : ITableIdentifier<'tgt>, [<ReflectedDefinition>] predicate : Quotations.Expr<'src1 -> 'src2 -> 'tgt -> bool>) =
        Join(x.Expression).InnerJoin(source1, source2, target, predicate)

    member x.LeftJoin<'src, 'tgt>(predicate : Expression<Func<'src, 'tgt, bool>>) = Join(x.Expression).LeftJoin(predicate)
    member x.LeftJoin<'src, 'tgt>([<ReflectedDefinition>] predicate : Quotations.Expr<'src -> 'tgt -> bool>) = Join(x.Expression).LeftJoin(predicate)

    member x.LeftJoin<'src1, 'src2, 'tgt>(predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) = Join(x.Expression).LeftJoin(predicate)
    member x.LeftJoin<'src1, 'src2, 'tgt>([<ReflectedDefinition>] predicate : Quotations.Expr<'src1 -> 'src2 -> 'tgt -> bool>) = Join(x.Expression).LeftJoin(predicate)

    member x.RightJoin<'src, 'tgt>(predicate : Expression<Func<'src, 'tgt, bool>>) = Join(x.Expression).RightJoin(predicate)
    member x.RightJoin<'src, 'tgt>([<ReflectedDefinition>] predicate : Quotations.Expr<'src -> 'tgt -> bool>) = Join(x.Expression).RightJoin(predicate)
    
    member x.RightJoin<'src1, 'src2, 'tgt>(predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) = Join(x.Expression).RightJoin(predicate)
    member x.RightJoin<'src1, 'src2, 'tgt>([<ReflectedDefinition>] predicate : Quotations.Expr<'src1 -> 'src2 -> 'tgt -> bool>) = Join(x.Expression).RightJoin(predicate)

    member x.FullJoin<'src, 'tgt>(predicate : Expression<Func<'src, 'tgt, bool>>) = Join(x.Expression).FullJoin(predicate)
    member x.FullJoin<'src, 'tgt>([<ReflectedDefinition>] predicate : Quotations.Expr<'src -> 'tgt -> bool>) = Join(x.Expression).FullJoin(predicate)

    member x.FullJoin<'src1, 'src2, 'tgt>(predicate : Expression<Func<'src1, 'src2, 'tgt, bool>>) = Join(x.Expression).FullJoin(predicate)
    member x.FullJoin<'src1, 'src2, 'tgt>([<ReflectedDefinition>] predicate : Quotations.Expr<'src1 -> 'src2 -> 'tgt -> bool>) = Join(x.Expression).FullJoin(predicate)

    member x.Where<'a>(predicate : Expression<Func<'a, bool>>) = SelectWhere(x.Expression).And predicate
    member x.Where<'a>([<ReflectedDefinition>] predicate : Quotations.Expr<'a -> bool>) = SelectWhere(x.Expression).And predicate
    member x.Where<'a, 'b>(predicate : Expression<Func<'a, 'b, bool>>) = SelectWhere(x.Expression).And predicate
    member x.Where<'a, 'b>([<ReflectedDefinition>] predicate : Quotations.Expr<'a -> 'b -> bool>) = SelectWhere(x.Expression).And predicate


    member x.GroupBy<'a>(selector : Expression<Func<'a, obj>>) = SelectGroupBy(x.Expression).ThenBy(selector)
    member x.GroupBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<('a -> 'b)>) = SelectGroupBy(x.Expression).ThenBy(selector)

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>) = SelectOrderBy(x.Expression).ThenBy(selector)
    member x.OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>) = SelectOrderBy(x.Expression).ThenBy(selector)

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction) = SelectOrderBy(x.Expression).ThenBy(selector, direction)
    member x.OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, direction : Direction) = SelectOrderBy(x.Expression).ThenBy(selector, direction)
    
    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction, nullsOrdering : NullsOrdering) = SelectOrderBy(x.Expression).ThenBy(selector, direction, nullsOrdering)
    member x.OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, direction : Direction, nullsOrdering : NullsOrdering) = SelectOrderBy(x.Expression).ThenBy(selector, direction, nullsOrdering)

type SelectImpl internal (expr : SelectValuesExpressionNode) =
    internal new () = SelectImpl(NewSelectExpression ())

    member x.Distinct 
        with get() = SelectImpl(MakeDistinct expr)
    
    member x.Star<'a>() = 
        SelectImpl(SelectAllColumns expr (TableIdentifier<'a>()))

    member x.Column<'a>(tbl : ITableIdentifier<'a>, selector : Expression<Func<'a, obj>>) = 
        SelectImpl(SelectColumns expr (LinqExpression(selector)) tbl)
    member x.Column<'a, 'b>(tbl : ITableIdentifier<'a>, selector : Quotations.Expr<('a -> 'b)>) =
        SelectImpl(SelectColumns expr (Quotation(selector)) tbl)

    member x.Column<'a>(selector : Expression<Func<'a, obj>>, partition : FluentOverPartitionBase) = 
        SelectImpl(SelectColumnWithPartition expr (LinqExpression(selector)) (TableIdentifier<'a>()) partition.Partition)
    member x.Column<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, partition : FluentOverPartitionBase) =
        SelectImpl(SelectColumnWithPartition expr (Quotation(selector)) (TableIdentifier<'a>()) partition.Partition)

    member x.Column<'a>(selector : Expression<Func<'a, obj>>) = x.Column(TableIdentifier<'a>(), selector)
    member x.Column<'a, 'b>(selector : Quotations.Expr<('a -> 'b)>) = x.Column(TableIdentifier<'a>(), selector)

    member x.From<'a>(tbl : ITableIdentifier<'a>) = SelectFrom<'a>(tbl, expr)
    member x.From<'a>() = x.From<'a>(TableIdentifier<'a>())

type Select private () =
    static member Distinct with get() = (SelectImpl()).Distinct
    static member Star<'a>() = (SelectImpl()).Star<'a>()

    static member Column<'a>(selector : Expression<Func<'a, obj>>) = (SelectImpl()).Column<'a>(selector)
    static member Column<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>) = (SelectImpl()).Column<'a, 'b>(selector)

    static member Column<'a>(selector : Expression<Func<'a, obj>>, partition : FluentOverPartitionBase) = (SelectImpl()).Column<'a>(selector, partition)
    static member Column<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, partition : FluentOverPartitionBase) = (SelectImpl()).Column<'a, 'b>(selector, partition)

    static member Column<'a>(tbl : ITableIdentifier<'a>, selector : Expression<Func<'a, obj>>) = (SelectImpl()).Column<'a>(tbl, selector)
