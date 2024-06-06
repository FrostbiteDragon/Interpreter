namespace FrostScript.Domain

type 'T IdMap = 
    {
        Values : Map<string, 'T> list
    }
    member this.Item
        with get id =
            match this.Values |> List.tryFindBack (fun x -> x.ContainsKey id) with
            | Some value -> value.[id]
            | None -> failwith "Key not found"

type 'T idMap = IdMap<'T>

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module IdMap =
    let ofList (list : Map<string, 'T> list) = 
        { Values = list } 

    let updateAt index id value (idMap : 'T idMap) : 'T idMap =
        idMap.Values
        |> List.updateAt index (idMap.Values[index] |> Map.change id (fun _ -> Some value))
        |> ofList

    let updateLocal id value (idMap : 'T idMap) = updateAt (idMap.Values.Length - 1) id value idMap

    let tryUpdate id value (idMap : 'T idMap) =
        let index = 
            idMap.Values
            |> List.tryFindIndexBack (fun x -> x.ContainsKey id)
       
        match index with
        | Some index -> Some (updateAt index id value idMap)
        | None -> None

    let update id value (idMap : 'T idMap) =
        let index = 
            idMap.Values
            |> List.tryFindIndexBack (fun x -> x.ContainsKey id)
       
        match index with
        | Some index -> updateAt index id value idMap
        | None -> updateLocal id value idMap

    let openLocal (idMap : 'T idMap) : 'T idMap = List.append idMap.Values [Map.empty] |> ofList
    let closeLocal (idMap : 'T idMap) : 'T idMap = idMap.Values |> List.removeAt (idMap.Values.Length - 1) |> ofList

    let useLocal<'T1, 'T2> (func : 'T1 idMap -> 'T2 * 'T1 idMap) (idMap : 'T1 idMap) =
        let idMap = idMap |> openLocal
        let (result, idMap) = func idMap
        (result, idMap |> closeLocal)

    let tryFind id (idMap : 'T idMap) = 
        let result = idMap.Values |> List.tryFind(fun x -> x.ContainsKey id)
        match result with
        | Some map -> Some map.[id]
        | None -> None