namespace Hyperboliq

open System
open System.Linq.Expressions
open Hyperboliq
open Hyperboliq.Types
open Hyperboliq.Domain.AST
open Hyperboliq.Domain.ExpressionParts
open Hyperboliq.Domain.ExpressionVisitor

[<AbstractClass>]
type FluentOverPartitionBase(p : WindowNode) =
    member x.Partition with internal get() = p

type EmptyOverStatement internal (p : WindowNode) =
    inherit FluentOverPartitionBase(p)

type OverOrderBy internal (p : WindowNode) =
    inherit FluentOverPartitionBase(p)

    static let New expr = OverOrderBy(expr)
    
    member internal x.InternalThenBy<'a>(selector : VisitableExpression, ?direction : Direction, ?nullsOrdering : NullsOrdering) =
        let dir = defaultArg direction Direction.Ascending
        let nullsOrder = defaultArg nullsOrdering NullsOrdering.NullsUndefined
        AddPartitionOrderBy x.Partition selector TableReferenceFromType<'a> dir nullsOrder

    member x.ThenBy<'a>(selector : Expression<Func<'a, obj>>) =
        x.InternalThenBy<'a>(LinqExpression(selector)) |> New
    member x.ThenBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>) =
        x.InternalThenBy<'a>(Quotation(selector)) |> New

    member x.ThenBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction) =
        x.InternalThenBy<'a>(LinqExpression(selector), direction) |> New
    member x.ThenBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, direction : Direction) =
        x.InternalThenBy<'a>(Quotation(selector), direction) |> New

    member x.ThenBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction, nullsOrder : NullsOrdering) =
        x.InternalThenBy<'a>(LinqExpression(selector), direction, nullsOrder) |> New
    member x.ThenBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, direction : Direction, nullsOrder : NullsOrdering) =
        x.InternalThenBy<'a>(Quotation(selector), direction, nullsOrder) |> New

type OverPartition internal (p : WindowNode) =
    inherit FluentOverPartitionBase(p)
    static let New expr = OverPartition(expr)

    member x.ThenBy<'a>(selector : Expression<Func<'a, obj>>) =
        AddPartitionBy x.Partition (LinqExpression(selector)) TableReferenceFromType<'a> |> New
    member x.ThenBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>) =
        AddPartitionBy x.Partition (Quotation(selector)) TableReferenceFromType<'a> |> New

    member x.OrderBy<'a>(selector : Expression<Func<'a, obj>>) = 
        OverOrderBy(x.Partition).ThenBy(selector)
    member x.OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>) =
        OverOrderBy(x.Partition).ThenBy(selector)

type Over private () =
    static let EmptyPartition = { PartitionBy = []; OrderBy = [] }
    static let EmptyOver = EmptyOverStatement(EmptyPartition)
    static let EmptyOverOrderBy = OverOrderBy(EmptyPartition)
    static let EmptyOverPartition = OverPartition(EmptyPartition)

    static member PartitionBy<'a>(selector : Expression<Func<'a, obj>>) = 
        EmptyOverPartition.ThenBy(selector)
    static member PartitionBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>) =
        EmptyOverPartition.ThenBy(selector)
    
    static member OrderBy<'a>(selector : Expression<Func<'a, obj>>) =
        EmptyOverOrderBy.ThenBy(selector)
    static member OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>) =
        EmptyOverOrderBy.ThenBy(selector)

    static member OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction) =
        EmptyOverOrderBy.ThenBy(selector, direction)
    static member OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, direction : Direction) =
        EmptyOverOrderBy.ThenBy(selector, direction)

    static member OrderBy<'a>(selector : Expression<Func<'a, obj>>, direction : Direction, nullsOrder : NullsOrdering) =
        EmptyOverOrderBy.ThenBy(selector, direction, nullsOrder)
    static member OrderBy<'a, 'b>([<ReflectedDefinition>] selector : Quotations.Expr<'a -> 'b>, direction : Direction, nullsOrder : NullsOrdering) =
        EmptyOverOrderBy.ThenBy(selector, direction, nullsOrder)

    static member Empty with get() = EmptyOver