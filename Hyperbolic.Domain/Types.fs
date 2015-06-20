namespace Hyperboliq.Domain

module Types =

    type ISqlDialect =
        abstract member CreateConnection : string -> System.Data.IDbConnection
        abstract member QuoteColumnName : string -> string

    type ISqlTransformable =
        abstract member ToSql : ISqlDialect -> string

    type ISqlQuery = 
        inherit ISqlTransformable
    
    type ISqlStatement =
        inherit ISqlTransformable

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

    type ITableReference = 
        abstract Table : System.Type with get
        abstract ReferenceName : string with get

    type ITableReference<'a> =
        inherit ITableReference
    
    [<StructuralEquality; NoComparison>]
    type internal TableReference<'a> = 
        { table : System.Type; referenceName : string }
        interface ITableReference<'a> with
            member x.Table with get() = x.table
            member x.ReferenceName with get() = x.referenceName
        member x.Table with get() = (x :> ITableReference<'a>).Table
        member x.ReferenceName with get() = (x :> ITableReference<'a>).ReferenceName
    
    type ITableDefinition =
        abstract Table : System.Type with get
        abstract Name : string with get

    type ITableDefinition<'a> =
        inherit ITableDefinition

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

    open System.Reflection;
    open System.Linq;

    let TableReferenceFromType<'a> = ({ table = typeof<'a>; referenceName = typeof<'a>.Name + "Ref" } : TableReference<'a>) :> ITableReference<'a>
    let NamedTableReferenceFromType<'a> name = ({ table = typeof<'a>; referenceName = name } : TableReference<'a>) :> ITableReference<'a>

    let TableDefinitionFromType<'a> = { table = typeof<'a>; name = None } :> ITableDefinition<'a>
    let NamedTableDefinitionFromType<'a> name = { table = typeof<'a>; name = Some name } :> ITableDefinition<'a>

    type ITableIdentifier =
        abstract member Definition : ITableDefinition with get 
        abstract member Reference : ITableReference with get

    type ITableIdentifier<'a> =
        inherit ITableIdentifier

    type TableIdentifier<'a> (def : ITableDefinition<'a>, ref : ITableReference<'a>) = 
        new () = TableIdentifier<'a>(TableDefinitionFromType<'a>, TableReferenceFromType<'a>)

        member x.Definition = def
        member x.Reference = ref
        
        override x.Equals o =
            match o with
            | :? ITableIdentifier<'a> as ti -> ti.Definition = (x.Definition :> ITableDefinition) && ti.Reference = (x.Reference :> ITableReference)
            | _ -> false
        override x.GetHashCode () = x.Definition.GetHashCode() + x.Reference.GetHashCode()

        interface ITableIdentifier<'a> with
            member x.Definition with get() = x.Definition :> ITableDefinition
            member x.Reference with get () = x.Reference :> ITableReference

    type BinaryOperation =
        Equal | NotEqual | GreaterThan | GreaterThanOrEqual | LessThan | LessThanOrEqual
        | In | And | Or | Add | Subtract | Multiply | Divide | Modulo | Coalesce

    open System.Linq.Expressions
    
    let ToBinaryOperation et =
        match et with
        | ExpressionType.Equal -> Equal
        | ExpressionType.NotEqual -> NotEqual
        | ExpressionType.GreaterThan -> GreaterThan
        | ExpressionType.GreaterThanOrEqual -> GreaterThanOrEqual
        | ExpressionType.LessThan -> LessThan
        | ExpressionType.LessThanOrEqual -> LessThanOrEqual
        | ExpressionType.AndAlso -> And
        | ExpressionType.OrElse -> Or
        | ExpressionType.Add -> Add
        | ExpressionType.Subtract -> Subtract
        | ExpressionType.Multiply -> Multiply
        | ExpressionType.Divide -> Divide
        | ExpressionType.Modulo -> Modulo
        | ExpressionType.Coalesce -> Coalesce
        | _ -> failwith "Not implemented"

