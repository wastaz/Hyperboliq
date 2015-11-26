namespace Hyperboliq.Domain

open System.Runtime.CompilerServices

[<assembly: InternalsVisibleTo("Hyperboliq.Ansi")>]
[<assembly: InternalsVisibleTo("Hyperboliq.Tests")>]
[<assembly: InternalsVisibleTo("Hyperboliq.Migration.Tests")>]
[<assembly: InternalsVisibleTo("Hyperboliq.Tests.FSharp")>]
do ()

module Array =
    let tryHead arr =
        if Array.isEmpty arr then None else Some (arr.[0])

    let sortByDescending selector arr =
        Array.sortBy selector arr |> Array.rev
        
module Seq = 
    let tryHead sq =
        if Seq.isEmpty sq then None else Some (Seq.head sq)

module List =
    let sortByDescending selector list =
        List.sortBy selector list |> List.rev

module Option =
    let OfObj (o : obj) =
        match o with
        | null -> None
        | _ -> Some o

    let UnwrapOrFail o =
        match o with
        | Some(value) -> value
        | None -> failwith "Not implemented"

module String =
    let join sep (strs : string list) =
        System.String.Join(sep, (List.toArray strs))

    let indexOf (str : string) (c : char) =
        str.IndexOf(c)