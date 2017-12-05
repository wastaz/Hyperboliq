namespace Hyperboliq

type Extension = System.Runtime.CompilerServices.ExtensionAttribute
type Optional = System.Runtime.InteropServices.OptionalAttribute
type DefaultParamValue = System.Runtime.InteropServices.DefaultParameterValueAttribute

open System
open System.Data
open System.Dynamic
open Dapper

[<Extension>]
type HyperboliqExtension () =
    static member private CreateParameterObject (parameters : ExpressionParameter array) =
        let exp = ExpandoObject()
        let d = exp :> System.Collections.Generic.IDictionary<string, obj>
        parameters
        |> Array.choose (fun p -> match p.Value with | Some(v) -> Some(p.Name, v) | None -> None)
        |> Array.iter d.Add
        exp

    [<Extension>]
    static member Query<'a>(con : HyperboliqConnection, 
                            query : ISqlQuery, 
                            [<Optional; DefaultParamValue(null)>] ?txn : IDbTransaction) =
        let txn' = defaultArg txn null
        let sql = query.ToSql(con.Dialect)
        con.Query<'a>(sql, null, txn')
       
    [<Extension>]
    static member Query<'a>(con : HyperboliqConnection, 
                            query : ISqlQuery, 
                            parameters : ExpressionParameter array, 
                            [<Optional; DefaultParamValue(null)>] ?txn : IDbTransaction) =
        let txn' = defaultArg txn null
        let sql = query.ToSql(con.Dialect)
        let paramObj = HyperboliqExtension.CreateParameterObject parameters
        con.Query<'a>(sql, paramObj, txn')

    [<Extension>]
    static member ExecuteScalar<'a>(con : HyperboliqConnection, 
                                    stmt : ISqlQuery, 
                                    [<Optional; DefaultParamValue(null)>] ?txn : IDbTransaction) =
        let txn' = defaultArg txn null
        let sql = stmt.ToSql(con.Dialect)
        con.ExecuteScalar<'a>(sql, null, txn')

    [<Extension>]
    static member ExecuteScalar<'a>(con : HyperboliqConnection, 
                                    stmt : ISqlQuery, 
                                    parameters : ExpressionParameter array, 
                                    [<Optional; DefaultParamValue(null)>] ?txn : IDbTransaction) =
        let txn' = defaultArg txn null
        let sql = stmt.ToSql(con.Dialect)
        let paramObj = HyperboliqExtension.CreateParameterObject parameters
        con.ExecuteScalar<'a>(sql, paramObj, txn')

    [<Extension>]
    static member ExecuteNonQuery(con : HyperboliqConnection, 
                                  stmt : ISqlStatement, 
                                  [<Optional; DefaultParamValue(null)>] ?txn : IDbTransaction) =
        let txn' = defaultArg txn null
        let sql = stmt.ToSql(con.Dialect)
        con.Execute(sql, null, txn')

    [<Extension>]
    static member ExecuteNonQuery(con : HyperboliqConnection, 
                                  stmt : ISqlStatement, 
                                  parameters : ExpressionParameter array, 
                                  [<Optional; DefaultParamValue(null)>] ?txn : IDbTransaction) =
        let txn' = defaultArg txn null
        let sql = stmt.ToSql(con.Dialect)
        let paramObj = HyperboliqExtension.CreateParameterObject parameters
        con.Execute(sql, paramObj, txn')
