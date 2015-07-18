namespace Hyperboliq

open System
open Hyperboliq.Domain.Stream
open Hyperboliq.Domain.SqlGen

type Union internal (expressions : IPlainSelectExpressionTransformable array) =
    let union = { 
        Operation = SetOperationType.Union
        Operands = expressions |> Array.map (fun e -> e.ToPlainSelectExpression()) |> Array.toList
    }

    member x.ToSelectExpression () = (x :> ISelectExpressionTransformable).ToSelectExpression ()
    interface ISelectExpressionTransformable with
        member x.ToSelectExpression () = Set(union)

    member x.ToSqlExpression () = (x :> ISqlExpressionTransformable).ToSqlExpression ()
    interface ISqlExpressionTransformable with
        member x.ToSqlExpression () = Select(x.ToSelectExpression ())

    member x.ToSql (dialect : ISqlDialect) = (x :> ISqlQuery).ToSql dialect
    interface ISqlQuery with
        member x.ToSql (dialect : ISqlDialect) = x.ToSqlExpression () |> SqlifyExpression dialect

type SetOperations private () =
    static member Union ([<System.ParamArray>] expressions : IPlainSelectExpressionTransformable array) = Union(expressions)
    static member UnionAll ([<System.ParamArray>] expressions : ISelectExpressionTransformable array) = ()
    static member Intersect (source : ISelectExpressionTransformable, [<System.ParamArray>] expressions : ISelectExpressionTransformable array) = ()
    static member Minus (source : ISelectExpressionTransformable, [<System.ParamArray>] expressions : ISelectExpressionTransformable array) = ()

