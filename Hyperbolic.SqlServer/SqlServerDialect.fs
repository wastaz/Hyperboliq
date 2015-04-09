namespace Hyperboliq.Dialects

open Hyperboliq.Domain.Types
open System.Data
open System.Data.SqlClient

type public SqlServer private () =
    static member private _dialect = lazy(new SqlServer())
    static member Dialect with get() = SqlServer._dialect.Value
    interface ISqlDialect with
        member x.QuoteColumnName colname = "[" + colname + "]"
        member x.CreateConnection connectionString = new SqlConnection(connectionString) :> IDbConnection