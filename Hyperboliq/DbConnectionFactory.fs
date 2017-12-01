namespace Hyperboliq

open Hyperboliq.Domain
open System.Dynamic

type HyperboliqConnection(dialect : ISqlDialect, connection : System.Data.IDbConnection) =
    let _dbConn = connection
    member val Dialect = dialect with get
    member x.AsIDbConnection() = x :> System.Data.IDbConnection
    interface System.Data.IDbConnection with
        member x.Open() = _dbConn.Open()
        member x.Close() = _dbConn.Close()
        member x.BeginTransaction() = _dbConn.BeginTransaction()
        member x.BeginTransaction(level) = _dbConn.BeginTransaction(level)
        member x.CreateCommand() = _dbConn.CreateCommand()
        member x.Dispose() = _dbConn.Dispose()
        member x.ChangeDatabase(dbName) = _dbConn.ChangeDatabase(dbName)
        member x.ConnectionString with get() = _dbConn.ConnectionString
        member x.ConnectionString with set(value) = _dbConn.ConnectionString <- value
        member x.ConnectionTimeout with get() = _dbConn.ConnectionTimeout
        member x.Database with get() = _dbConn.Database
        member x.State with get() = _dbConn.State

type HyperboliqConnectionFactory(dialect : ISqlDialect, connectionString : string) =
    member private x.Dialect = dialect
    member val ConnectionString = connectionString with get
    member x.OpenDbConnection() =
        let con = dialect.CreateConnection(x.ConnectionString)
        con.Open()
        new HyperboliqConnection(x.Dialect, con)
