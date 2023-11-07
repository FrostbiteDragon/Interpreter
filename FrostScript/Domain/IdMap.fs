namespace FrostScript

type IdMap<'T> = 
    {
        values : Map<string, 'T> list
    }
    member this.Item
        with get id =
            match this.values |> List.tryFindBack (fun x -> x.ContainsKey id) with
            | Some value -> value.[id]
            | None -> failwith "Key not found"
       

type 'T idMap = IdMap<'T>

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module IdMap =
    let ofList (list : Map<string, 'T> list) = 
        { values = list } 

    let updateAt index id value (idMap : 'T idMap) : 'T idMap =
        idMap.values
        |> List.updateAt index (idMap.values[index] |> Map.change id (fun _ -> Some value))
        |> ofList

    let updateLocal id value (idMap : 'T idMap) = updateAt (idMap.values.Length - 1) id value idMap

    let tryUpdate id value (idMap : 'T idMap) =
        let index = 
            idMap.values
            |> List.tryFindIndexBack (fun x -> x.ContainsKey id)
       
        match index with
        | Some index -> Some (updateAt index id value idMap)
        | None -> None

    let update id value (idMap : 'T idMap) =
        let index = 
            idMap.values
            |> List.tryFindIndexBack (fun x -> x.ContainsKey id)
       
        match index with
        | Some index -> updateAt index id value idMap
        | None -> updateLocal id value idMap

    let openLocal (idMap : 'T idMap) : 'T idMap = List.append idMap.values [Map.empty] |> ofList
    let closeLocal (idMap : 'T idMap) : 'T idMap = idMap.values |> List.removeAt (idMap.values.Length - 1) |> ofList

    let useLocal<'T1, 'T2> (func : 'T1 idMap -> 'T2 * 'T1 idMap) (idMap : 'T1 idMap) =
        let idMap = idMap |> openLocal
        let (result, idMap) = func idMap
        (result, idMap |> closeLocal)

    let tryFind id (idMap : 'T idMap) = 
        let result = idMap.values |> List.tryFind(fun x -> x.ContainsKey id)
        match result with
        | Some map -> Some map.[id]
        | None -> None

    //type IdentifierMap<'T> = 
    //{ globalIds : Map<string, 'T>
    //  localIds : Map<string, 'T> }

    //member this.Item
    //    with get id =
    //        match this.localIds.TryFind id with
    //        | Some value -> value
    //        | None -> this.globalIds.[id]

    //member this.Ids = 
    //    this.localIds
    //    |> Map.fold (fun newMap key value -> Map.add key value newMap) this.globalIds

    //member this.TryFind id =
    //    match this.localIds.TryFind id with
    //    | Some value -> Some value
    //    | None -> this.globalIds.TryFind id

    //member this.TryUpdate id value =
    //    match this.localIds.ContainsKey id with
    //    | true -> Some { this with localIds = this.localIds.Change(id, fun _ -> Some (value)) }
    //    | false -> 
    //        match this.globalIds.ContainsKey id with
    //        | true -> Some { this with globalIds = this.globalIds.Change(id, fun _ -> Some (value)) }
    //        | false -> None

    //member this.ChangeLocal id value =
    //    { this with localIds = this.localIds.Change (id, fun _ -> Some value) }

    //member this.Change id value =
    //    let withUpdatedLocalIds = { this with localIds = this.localIds.Change(id, fun _ -> Some (value)) }
    //    match this.localIds.ContainsKey id with
    //    | true -> withUpdatedLocalIds
    //    | false -> 
    //        match this.globalIds.ContainsKey id with
    //        | true -> { this with globalIds = this.globalIds.Change(id, fun _ -> Some (value)) }
    //        | false -> withUpdatedLocalIds