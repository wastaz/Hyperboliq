namespace Hyperboliq.Domain

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
