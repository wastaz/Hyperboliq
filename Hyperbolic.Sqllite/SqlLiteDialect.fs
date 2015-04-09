namespace Hyperboliq.Dialects

open Hyperboliq.Domain.Types
open System.Data
open System.Data.SQLite

type public SqlLite private () =
    static member private _dialect = lazy(new SqlLite())
    static member Dialect with get() = SqlLite._dialect.Value
    interface ISqlDialect with
        member x.CreateConnection connectionString = new SQLiteConnection(connectionString) :> IDbConnection
        member x.QuoteColumnName colname = @"""" + colname + @""""
