namespace Jarvis.Analysis

open System.Collections.Generic
open Jarvis

type Breakdown = Dictionary<Activity, int> // int = seconds

module Breakdown =

    let rec private add_activity (b: Breakdown) (activity: Activity) =
        match activity with
        | _ :: xs ->
            if b.ContainsKey activity then
                b.[activity] <- b.[activity] + 10
            else
                b.Add(activity, 10)
            add_activity b xs
        | [] -> ()

    let format_seconds (seconds: int) =
        let mutable seconds = seconds
        let mutable output = ""
    
        let divmod (n: int) (suffix: string) =
            if seconds > n then
                output <- output + sprintf "%i%s" (seconds / n) suffix
                seconds <- seconds % n

        divmod 86400 "d"
        divmod 3600 "h"
        divmod 60 "m"
        divmod 1 "s"

        output

    let print(b: Breakdown) =
        for k in b.Keys |> Seq.sortByDescending (fun k -> b.[k]) |> Seq.truncate 1000 do
            printfn "%-150s | %s" (String.replicate k.Length "  " + String.concat " > " (List.rev k)) (format_seconds b.[k])

    let create(slice: Activity array) =
        let breakdown = new Breakdown()
        Array.iter (add_activity breakdown) slice
        breakdown