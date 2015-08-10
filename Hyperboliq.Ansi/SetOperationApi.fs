namespace Hyperboliq

open System
open Hyperboliq.Domain.Stream
open Hyperboliq.Domain.SqlGen

type OtherSetOperation internal (opType : SetOperationType, expressions : IPlainSelectExpressionTransformable list) =
    let op = {
        Operation = opType
        Operands = expressions |> List.map (fun e -> e.ToPlainSelectExpression())
    }

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

type Union internal (expressions : IPlainSelectExpressionTransformable array) =
    let union = { 
        Operation = SetOperationType.Union
        Operands = expressions |> Array.map (fun e -> e.ToPlainSelectExpression()) |> Array.toList
    }

    member x.Union ([<System.ParamArray>] additional : IPlainSelectExpressionTransformable array) =
        [ expressions; additional ] 
        |> Array.concat
        |> (fun ex -> Union(ex))

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
    static member Union ([<System.ParamArray>] expressions : IPlainSelectExpressionTransformable array) = 
        Union(expressions)
    static member UnionAll ([<System.ParamArray>] expressions : IPlainSelectExpressionTransformable array) = ()
    static member Intersect (source : IPlainSelectExpressionTransformable, [<System.ParamArray>] expressions : IPlainSelectExpressionTransformable array) = 
        OtherSetOperation(SetOperationType.Intersect, source :: (List.ofArray expressions))
    static member Minus (source : IPlainSelectExpressionTransformable, [<System.ParamArray>] expressions : IPlainSelectExpressionTransformable array) = 
        OtherSetOperation(SetOperationType.Minus, source :: (List.ofArray expressions))

