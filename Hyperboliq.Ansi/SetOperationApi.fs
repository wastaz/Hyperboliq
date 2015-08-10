namespace Hyperboliq

open System
open Hyperboliq.Domain.AST
open Hyperboliq.Domain.SqlGen

type OtherSetOperation internal (opType : SetOperationType, expressions : IPlainSelectExpressionTransformable list) =
    let op = {
        Operation = opType
        Operands = expressions |> List.map (fun e -> e.ToPlainSelectExpression())
    }

    member x.Union ([<System.ParamArray>] additional : IPlainSelectExpressionTransformable array) =
        Array.append [| (x.ToPlainSelectExpression() :> IPlainSelectExpressionTransformable) |] additional
        |> fun a -> Union(false, a)

    member x.UnionAll ([<System.ParamArray>] additional : IPlainSelectExpressionTransformable array) =
        Array.append [| (x.ToPlainSelectExpression() :> IPlainSelectExpressionTransformable) |] additional
        |> fun a -> Union(true, a)

    member x.Intersect ([<System.ParamArray>] additional : IPlainSelectExpressionTransformable array) =
        OtherSetOperation(SetOperationType.Intersect, (x.ToPlainSelectExpression() :> IPlainSelectExpressionTransformable) :: (Array.toList additional))

    member x.Minus ([<System.ParamArray>] additional : IPlainSelectExpressionTransformable array) =
        OtherSetOperation(SetOperationType.Minus, (x.ToPlainSelectExpression() :> IPlainSelectExpressionTransformable) :: (Array.toList additional))
    
    member x.ToPlainSelectExpression () = (x :> IPlainSelectExpressionTransformable).ToPlainSelectExpression()
    interface IPlainSelectExpressionTransformable with
        member x.ToPlainSelectExpression () = PlainSelectExpression.Set(op)
        
    member x.ToSelectExpression () = (x :> ISelectExpressionTransformable).ToSelectExpression ()
    interface ISelectExpressionTransformable with
        member x.ToSelectExpression () = SelectExpression.Plain(x.ToPlainSelectExpression ())

    member x.ToSqlExpression () = (x :> ISqlExpressionTransformable).ToSqlExpression ()
    interface ISqlExpressionTransformable with
        member x.ToSqlExpression () = Select(x.ToSelectExpression ())

    member x.ToSql (dialect : ISqlDialect) = (x :> ISqlQuery).ToSql dialect
    interface ISqlQuery with
        member x.ToSql (dialect : ISqlDialect) = x.ToSqlExpression () |> SqlifyExpression dialect

and Union internal (isUnionAll : bool, expressions : IPlainSelectExpressionTransformable array) =
    let union = { 
        Operation = if isUnionAll then SetOperationType.UnionAll else SetOperationType.Union
        Operands = expressions |> Array.map (fun e -> e.ToPlainSelectExpression()) |> Array.toList
    }

    let createUnion isUnionAll additional =
        [ expressions; additional ] 
        |> Array.concat
        |> (fun ex -> Union(isUnionAll, ex))

    member x.Union ([<System.ParamArray>] additional : IPlainSelectExpressionTransformable array) = createUnion false additional
    member x.UnionAll ([<System.ParamArray>] additional : IPlainSelectExpressionTransformable array) = createUnion true additional

    member x.ToPlainSelectExpression () = (x :> IPlainSelectExpressionTransformable).ToPlainSelectExpression ()
    interface IPlainSelectExpressionTransformable with
        member x.ToPlainSelectExpression () = PlainSelectExpression.Set(union)

    member x.ToSelectExpression () = (x :> ISelectExpressionTransformable).ToSelectExpression ()
    interface ISelectExpressionTransformable with
        member x.ToSelectExpression () = SelectExpression.Plain(x.ToPlainSelectExpression ())

    member x.ToSqlExpression () = (x :> ISqlExpressionTransformable).ToSqlExpression ()
    interface ISqlExpressionTransformable with
        member x.ToSqlExpression () = Select(x.ToSelectExpression ())

    member x.ToSql (dialect : ISqlDialect) = (x :> ISqlQuery).ToSql dialect
    interface ISqlQuery with
        member x.ToSql (dialect : ISqlDialect) = x.ToSqlExpression () |> SqlifyExpression dialect

type SetOperations private () =
    static member Union ([<System.ParamArray>] expressions : IPlainSelectExpressionTransformable array) = Union(false, expressions)
    static member UnionAll ([<System.ParamArray>] expressions : IPlainSelectExpressionTransformable array) = Union(true, expressions)
    static member Intersect (source : IPlainSelectExpressionTransformable, [<System.ParamArray>] expressions : IPlainSelectExpressionTransformable array) = 
        OtherSetOperation(SetOperationType.Intersect, source :: (List.ofArray expressions))
    static member Minus (source : IPlainSelectExpressionTransformable, [<System.ParamArray>] expressions : IPlainSelectExpressionTransformable array) = 
        OtherSetOperation(SetOperationType.Minus, source :: (List.ofArray expressions))

