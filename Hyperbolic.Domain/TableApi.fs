namespace Hyperboliq

open Hyperboliq.Types

[<AbstractClass>]
type TableReferenceCreator<'a> internal () =
    abstract ToTableReference : unit -> TableIdentifier<'a>
    interface ITableIdentifier<'a> with
        member x.Definition with get() = x.ToTableReference().Definition :> ITableDefinition
        member x.Reference with get() = x.ToTableReference().Reference :> ITableReference

type NamedTableWithReferenceName<'a> internal (tblName : string, refName : string) =
    inherit TableReferenceCreator<'a>() 
    override x.ToTableReference() =
        let tdef = NamedTableDefinitionFromType<'a> tblName
        let tref = NamedTableReferenceFromType<'a> refName
        TableIdentifier<'a>(tdef, tref)

type TableWithReferenceName<'a> internal (name : string) =
    inherit TableReferenceCreator<'a>()
    override x.ToTableReference() =
        let tdef = TableDefinitionFromType<'a>
        let tref = NamedTableReferenceFromType<'a> name
        TableIdentifier<'a>(tdef, tref)

type NamedTable<'a> internal (tblName : string) =
    inherit TableReferenceCreator<'a>()
    member x.WithReferenceName (refName : string) = NamedTableWithReferenceName<'a>(tblName, refName)
    
    override x.ToTableReference() = 
        let tdef = NamedTableDefinitionFromType<'a> tblName
        let tref = NamedTableReferenceFromType<'a> (tblName + "Ref")
        TableIdentifier<'a>(tdef, tref)

type Table<'a> private () =
    static member WithTableAlias (alias : string) = NamedTable<'a>(alias)
    static member WithReferenceName (name : string) = TableWithReferenceName<'a>(name)
