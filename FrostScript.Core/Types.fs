namespace FrostScript.Core

type IdentifierMap<'T> = 
    { globalIds : Map<string, 'T>
      localIds : Map<string, 'T> }

    member this.Item
        with get id =
            match this.localIds.TryFind id with
            | Some value -> value
            | None -> this.globalIds.[id]

    member this.Ids = 
        this.localIds
        |> Map.fold (fun newMap key value -> Map.add key value newMap) this.globalIds

    member this.TryFind id =
        match this.localIds.TryFind id with
        | Some value -> Some value
        | None -> this.globalIds.TryFind id

    member this.TryUpdate id value =
        match this.localIds.ContainsKey id with
        | true -> Some { this with localIds = this.localIds.Change(id, fun _ -> Some (value)) }
        | false -> 
            match this.globalIds.ContainsKey id with
            | true -> Some { this with globalIds = this.globalIds.Change(id, fun _ -> Some (value)) }
            | false -> None

    member this.ChangeLocal id value =
        { this with localIds = this.localIds.Change (id, fun _ -> Some value) }

    member this.Change id value =
        let withUpdatedLocalIds = { this with localIds = this.localIds.Change(id, fun _ -> Some (value)) }
        match this.localIds.ContainsKey id with
        | true -> withUpdatedLocalIds
        | false -> 
            match this.globalIds.ContainsKey id with
            | true -> { this with globalIds = this.globalIds.Change(id, fun _ -> Some (value)) }
            | false -> withUpdatedLocalIds