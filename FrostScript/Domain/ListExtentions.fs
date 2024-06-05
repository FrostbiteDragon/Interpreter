module FrostScript.Domain.List
     /// Map a Result producing function over a list to get a new Result
    /// using applicative style
    /// ('a -> Result<'b>) -> 'a list -> Result<'b list>
    let traverseResult f list =
        let applyResults fResult xResult =
            match fResult,xResult with
            | Ok f, Ok x ->
                Ok (f x)
            | Error errs, Ok x ->
                Error errs
            | Ok f, Error errs ->
                Error errs
            | Error errs1, Error errs2 ->
                // concat both lists of errors
                Error (List.concat [errs1; errs2])



        // define the applicative functions
        let (<*>) = applyResults
        let retn = Ok

        // define a "cons" function
        let cons head tail = head :: tail

        // right fold over the list
        let initState = retn []
        let folder head tail =
            retn cons <*> (f head) <*> tail

        List.foldBack folder list initState

    let traverseOption f list =
        let applyResults fResult xResult =
            match fResult, xResult with
            | Some f, Some x ->
                Some (f x)
            | None, Some _ ->
                None
            | Some _, None ->
                None
            | None, None ->
                None


        // define the applicative functions
        let (<*>) = applyResults
        let retn = Some

        // define a "cons" function
        let cons head tail = head :: tail

        // right fold over the list
        let initState = retn []
        let folder head tail =
            retn cons <*> (f head) <*> tail

        List.foldBack folder list initState

    let sequenceResult list = traverseResult id list
    let sequenceOption list = traverseOption id list
