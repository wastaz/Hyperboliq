namespace Hyperboliq

type Extension = System.Runtime.CompilerServices.ExtensionAttribute

[<Extension>]
module DapperIntegration =
    open System
    open System.Data
    open System.Dynamic
    open Dapper

    let private CreateParameterObject (parameters : ExpressionParameter array) =
        let exp = ExpandoObject()
        let d = exp :> System.Collections.Generic.IDictionary<string, obj>
        parameters
        |> Array.choose (fun p -> match p.Value with | Some(v) -> Some(p.Name, v) | None -> None)
        |> Array.iter d.Add
        exp

    [<Extension>]
    let Query<'a> 
        (con : HyperboliqConnection)
        (query : ISqlQuery) 
        ([<ParamArray>] parameters : ExpressionParameter array) : 'a seq  =
            let sql = query.ToSql(con.Dialect)
            let paramObj = CreateParameterObject parameters
            con.Query<'a>(sql, paramObj)

    [<Extension>]
    let ExecuteNonQuery
        (con : HyperboliqConnection)
        (stmt : ISqlStatement)
        ([<ParamArray>] parameters : ExpressionParameter array) =
            let sql = stmt.ToSql(con.Dialect)
            let paramObj = CreateParameterObject parameters
            con.Execute(sql, paramObj)

    [<Extension>]
    let ExecuteScalar<'a>
        (con : HyperboliqConnection)
        (stmt : ISqlQuery)
        ([<ParamArray>] parameters : ExpressionParameter array) =
            let sql = stmt.ToSql(con.Dialect)
            let paramObj = CreateParameterObject parameters
            con.ExecuteScalar<'a>(sql, paramObj)
