namespace Hyperboliq.Dialects

open Hyperboliq.Domain.Types

type public AnsiSql private () =
    static member private _dialect = lazy(new AnsiSql())
    static member Dialect with get() = AnsiSql._dialect.Value
    interface ISqlDialect with
        member x.CreateConnection connectionString = null
        member x.QuoteColumnName colname = colname