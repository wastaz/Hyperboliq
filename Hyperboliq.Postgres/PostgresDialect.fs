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