namespace Hyperboliq

type ISqlDialect =
    abstract member CreateConnection : string -> System.Data.IDbConnection
    abstract member QuoteIdentifier : string -> string

type ISqlTransformable =
    abstract member ToSql : ISqlDialect -> string

type ISqlQuery = 
    inherit ISqlTransformable
    
type ISqlStatement =
    inherit ISqlTransformable

    
type ExpressionVisitorConfig = {
    IsUpdate : bool
}

type ExpressionParameter(name : string) =
    member val internal Value = None with get, set
    member x.Name = name
    member x.HasValue () = x.Value.IsSome
    member x.SetValue o = x.Value <- Some(o)
    override x.Equals(obj) =
        match obj with
        | :? ExpressionParameter as other -> x.Name.Equals(other.Name)
        | _ -> false
    override x.GetHashCode() = x.Name.GetHashCode()

type ExpressionParameter<'TParamType> (name : string) =
    inherit ExpressionParameter(name)
    member x.SetValue (o : 'TParamType) = x.Value <- Some(o :> obj)
    static member op_Implicit(x : ExpressionParameter<'TParamType>) = Unchecked.defaultof<'TParamType>

type ITableReference = 
    abstract Schema : string option with get
    abstract Table : System.Type with get
    abstract ReferenceName : string with get

type ITableReference<'a> =
    inherit ITableReference

type ITableDefinition =
    abstract Schema : string option with get
    abstract Table : System.Type with get
    abstract Name : string with get

type ITableDefinition<'a> =
    inherit ITableDefinition

 type SchemaAttribute(schema : string) =
    inherit System.Attribute()
    member val Schema = schema with get

module internal Types =
    open System.Reflection;
    open System.Linq;

    [<StructuralEquality; NoComparison>]
    type internal TableReference<'a> = 
        { table : System.Type
          schema : string option
          referenceName : string
        }
        member x.Table with get() = (x :> ITableReference<'a>).Table
        member x.ReferenceName with get() = (x :> ITableReference<'a>).ReferenceName
        member x.Schema with get() = (x :> ITableReference<'a>).Schema

        interface ITableReference<'a> with
            member x.Table with get() = x.table
            member x.ReferenceName with get() = x.referenceName
            member x.Schema with get() = x.schema


    [<StructuralEquality; NoComparison>]
    type internal TableDefinition<'a> =
        { schema : string option
          table : System.Type
          name : string option 
        }
        member x.Table with get() = (x :> ITableDefinition<'a>).Table
        member x.Name with get() = (x :> ITableDefinition<'a>).Name
        member x.Schema with get() = (x :> ITableDefinition<'a>).Schema

        interface ITableDefinition<'a> with
            member x.Table with get() = x.table
            member x.Schema with get() = x.schema
            member x.Name with get() = match x.name with
                                        | None -> x.Table.Name
                                        | Some name -> name

    let private createTableReference t =
        { table = t
          referenceName = t.Name + "Ref"
          schema = None 
        }
        |> fun ref ->
          match t.GetCustomAttribute(typeof<SchemaAttribute>) with
          | :? SchemaAttribute as a -> { ref with schema = Some a.Schema }
          | _ -> ref

    let private createTableDefinition t =
        { table = t
          name = None
          schema = None
        }
        |> fun def ->
          match t.GetCustomAttribute(typeof<SchemaAttribute>) with
          | :? SchemaAttribute as a -> { def with schema = Some a.Schema }
          | _ -> def

    let TableReferenceFromType<'a> = 
        createTableReference typeof<'a> :> ITableReference<'a>

    let NamedTableReferenceFromType<'a> name =
        createTableReference typeof<'a>
        |> fun ref -> { ref with referenceName = name } :> ITableReference<'a>

    let TableDefinitionFromType<'a> = 
        createTableDefinition typeof<'a> :> ITableDefinition<'a>
        
    let NamedTableDefinitionFromType<'a> name = 
        createTableDefinition typeof<'a>
        |> fun def -> { def with name = Some name } :> ITableDefinition<'a>
    
