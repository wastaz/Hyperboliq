namespace Hyperboliq.Dialects

open Hyperboliq
open System.Data
open System.Data.SqlClient
open Npgsql

type public PostgreSql private () =
    static member private _dialect = lazy(new PostgreSql())
    static member Dialect with get() = PostgreSql._dialect.Value
    interface ISqlDialect with
        member x.QuoteIdentifier identifier = sprintf "\"%s\"" identifier
        member x.CreateConnection connectionString = new NpgsqlConnection(connectionString) :> IDbConnection
        member x.GenerateLimitOffsetSql limit offset =
            match limit, offset with
            | Some(limit), Some(offset) -> 
                sprintf "LIMIT %i OFFSET %i" limit offset |> Some
            | Some(limit), None ->
                sprintf "LIMIT %i" limit |> Some
            | None, Some(offset) ->
                sprintf "OFFSET %i" offset |> Some
            | _ -> 
                None
