namespace Hyperboliq

open Hyperboliq.Domain
open System.Dynamic

type AliasAttribute(alias : string) =
    inherit System.Attribute()
    member val Alias = alias with get

type IColumnMappingStrategy =
    abstract member MapColumn : System.Reflection.PropertyInfo seq -> string -> System.Reflection.PropertyInfo option

type internal PlainNameMapper() =
    interface IColumnMappingStrategy with
        member x.MapColumn properties colName = 
            query {
                for property in properties do
                where (property.Name = colName)
                select property
            } 
            |> Seq.tryHead

type internal AliasNameMapper() =
    interface IColumnMappingStrategy with
        member x.MapColumn properties colName =
            let property = 
                properties
                |> Seq.map (fun prop -> 
                    let attribute = 
                        prop.GetCustomAttributes(false)
                        |> Array.filter (fun attr -> attr.GetType() = typeof<AliasAttribute>)
                        |> Array.tryHead
                    match attribute with
                    | Some(attr) -> prop, Some(attr :?> AliasAttribute)
                    | None -> prop, None)
                |> Seq.choose (fun (prop, attr) -> 
                    match attr with
                    | Some(a) -> Some(prop, a)
                    | None -> None)
                |> Seq.tryFind (fun (prop, attr) -> attr.Alias = colName)
            match property with
            | Some(p, a) -> Some(p)
            | None -> None

type DynamicQueryResult() =
    inherit System.Dynamic.DynamicObject()
    let dict = new System.Collections.Generic.Dictionary<string, obj>()
    override __.TryGetMember (binder : GetMemberBinder, result : byref<obj>) =
        if dict.ContainsKey binder.Name then
            result <- dict.[binder.Name]
            true
        else base.TryGetMember(binder, &result)
    member internal x.SetMember (name : string) (value : obj) =
        dict.[name] <- value

type HyperboliqConnection(dialect : ISqlDialect, connection : System.Data.IDbConnection) =
    let _dbConn = connection
    member val Dialect = dialect with get
    member x.AsIDbConnection() = x :> System.Data.IDbConnection
    member val NamingStrategies = 
        [ 
            AliasNameMapper() :> IColumnMappingStrategy;
            PlainNameMapper() :> IColumnMappingStrategy;
        ] with get
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
        let con = dialect.CreateConnection("Data Source=" + x.ConnectionString)
        con.Open()
        new HyperboliqConnection(x.Dialect, con)

module internal Mapper =
    open System.Data

    let private MapRow<'a> (reader : IDataReader) (namingStrategies : IColumnMappingStrategy seq) : 'a =
        let instance = System.Activator.CreateInstance<'a>()
        let targetType = typeof<'a>
        let properties = targetType.GetProperties()
        for i = 0 to reader.FieldCount - 1 do
            let name = reader.GetName(i)
            let value = reader.GetValue(i)
            let prop =
                namingStrategies
                |> Seq.choose (fun strat -> strat.MapColumn properties name)
                |> Seq.tryHead
            match prop with
            | Some(p) -> p.GetSetMethod().Invoke(instance, [| value |]) |> ignore
            | None -> ()
        instance

    let private DynamicMapRow (reader : IDataReader) : DynamicQueryResult =
        let res = new DynamicQueryResult()
        for i = 0 to reader.FieldCount - 1 do
            let name = reader.GetName(i)
            let value = reader.GetValue(i)
            res.SetMember name value
        res

    let Map<'a> (reader : IDataReader) (namingStrategies : IColumnMappingStrategy seq) : 'a seq =
        let result = new System.Collections.Generic.List<'a>()
        while reader.Read() do
            let current = MapRow reader namingStrategies
            result.Add(current)
        result :> System.Collections.Generic.IEnumerable<'a>

    let DynamicMap (reader : IDataReader) : DynamicQueryResult seq =
        let result = new System.Collections.Generic.List<DynamicQueryResult>()
        while reader.Read() do
            let current = DynamicMapRow reader
            result.Add(current)
        result :> System.Collections.Generic.IEnumerable<DynamicQueryResult>

[<System.Runtime.CompilerServices.Extension>]
module IDbConnectionExtensions =
    open System.Data
    type Extension = System.Runtime.CompilerServices.ExtensionAttribute

    let private PrepareCommand (con : HyperboliqConnection) (query : ISqlTransformable) ([<System.ParamArray>] parameters : ExpressionParameter array) =
        let cmd = (con :> IDbConnection).CreateCommand()
        cmd.CommandText <- query.ToSql(con.Dialect)
        for param in parameters do
            match param.Value with
            | Some(value) -> 
                let p = cmd.CreateParameter()
                p.ParameterName <- param.Name
                p.Value <- value
                cmd.Parameters.Add(p) |> ignore
            | None -> ()
        cmd

    [<Extension>]
    let ExecuteNonQuery (con : HyperboliqConnection) (query : ISqlStatement) ([<System.ParamArray>] parameters : ExpressionParameter array)  =
        let cmd = PrepareCommand con query parameters
        cmd.ExecuteNonQuery()

    [<Extension>]
    let ExecuteScalar (con : HyperboliqConnection) (query : ISqlQuery) ([<System.ParamArray>] parameters : ExpressionParameter array)  =
        let cmd = PrepareCommand con query parameters
        cmd.ExecuteScalar()

    [<Extension>]
    let Query<'a> (con : HyperboliqConnection) (query : ISqlQuery) ([<System.ParamArray>] parameters : ExpressionParameter array) : 'a seq =
        let cmd = PrepareCommand con query parameters
        use reader = cmd.ExecuteReader()
        Mapper.Map<'a> reader con.NamingStrategies

    [<Extension>]
    let DynamicQuery (con : HyperboliqConnection) (query : ISqlQuery) ([<System.ParamArray>] parameters : ExpressionParameter array) : DynamicQueryResult seq =
        let cmd = PrepareCommand con query parameters
        use reader = cmd.ExecuteReader()
        Mapper.DynamicMap reader
