namespace Hyperboliq.Dialects

open Hyperboliq

type public AnsiSql private () =
    static member private _dialect = lazy(new AnsiSql())
    static member Dialect with get() = AnsiSql._dialect.Value
    interface ISqlDialect with
        member x.CreateConnection connectionString = null
        member x.QuoteIdentifier identifier = identifier
        member x.GenerateLimitOffsetSql limit offset = None