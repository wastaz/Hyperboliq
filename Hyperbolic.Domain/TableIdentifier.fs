namespace Hyperboliq

open Types

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
    | :? ITableIdentifier<'a> as ti -> 
      ti.Definition = (x.Definition :> ITableDefinition) && 
      ti.Reference = (x.Reference :> ITableReference)
    | _ -> false
  override x.GetHashCode () = 
    x.Definition.GetHashCode() + x.Reference.GetHashCode()

  interface ITableIdentifier<'a> with
    member x.Definition with get() = x.Definition :> ITableDefinition
    member x.Reference with get () = x.Reference :> ITableReference



