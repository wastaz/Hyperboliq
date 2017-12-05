namespace Hyperboliq.Dialects

open Hyperboliq
open System.Data
open System.Data.SqlClient

type public SqlServer private () =
    static member private _dialect = lazy(new SqlServer())
    static member Dialect with get() = SqlServer._dialect.Value
    interface ISqlDialect with
        member x.QuoteIdentifier identifier = sprintf "[%s]" identifier
        member x.CreateConnection connectionString = new SqlConnection(connectionString) :> IDbConnection
        member x.GenerateLimitOffsetSql limit offset =
            match limit, offset with
            | Some(limit), Some(offset) -> 
                sprintf "OFFSET %i ROWS FETCH NEXT %i ROWS ONLY" offset limit |> Some
            | Some(limit), None ->
                sprintf "OFFSET 0 ROWS FETCH NEXT %i ROWS ONLY" limit |> Some
            | None, Some(offset) ->
                sprintf "OFFSET %i ROWS" offset |> Some
            | _ -> 
                None
