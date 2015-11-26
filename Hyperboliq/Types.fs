namespace Hyperboliq

type ISqlDialect =
    abstract member CreateConnection : string -> System.Data.IDbConnection
    abstract member QuoteColumnName : string -> string

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
    abstract Table : System.Type with get
    abstract ReferenceName : string with get

type ITableReference<'a> =
    inherit ITableReference

type ITableDefinition =
    abstract Table : System.Type with get
    abstract Name : string with get

type ITableDefinition<'a> =
    inherit ITableDefinition


module internal Types =
    open System.Reflection;
    open System.Linq;

    [<StructuralEquality; NoComparison>]
    type internal TableReference<'a> = 
        { table : System.Type; referenceName : string }
        member x.Table with get() = (x :> ITableReference<'a>).Table
        member x.ReferenceName with get() = (x :> ITableReference<'a>).ReferenceName

        interface ITableReference<'a> with
            member x.Table with get() = x.table
            member x.ReferenceName with get() = x.referenceName


    [<StructuralEquality; NoComparison>]
    type internal TableDefinition<'a> =
        { table : System.Type; name : string option }
        member x.Table with get() = (x :> ITableDefinition<'a>).Table
        member x.Name with get() = (x :> ITableDefinition<'a>).Name

        interface ITableDefinition<'a> with
            member x.Table with get() = x.table
            member x.Name with get() = match x.name with
                                        | None -> x.Table.Name
                                        | Some name -> name

    let TableReferenceFromType<'a> = ({ table = typeof<'a>; referenceName = typeof<'a>.Name + "Ref" } : TableReference<'a>) :> ITableReference<'a>
    let NamedTableReferenceFromType<'a> name = ({ table = typeof<'a>; referenceName = name } : TableReference<'a>) :> ITableReference<'a>

    let TableDefinitionFromType<'a> = { table = typeof<'a>; name = None } :> ITableDefinition<'a>
    let NamedTableDefinitionFromType<'a> name = { table = typeof<'a>; name = Some name } :> ITableDefinition<'a>
