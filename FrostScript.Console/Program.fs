open FrostScript

match FrostScript.execute "test" with
| Some value -> printfn "%O" value
| None -> ()
