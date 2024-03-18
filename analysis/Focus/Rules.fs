namespace Jarvis.Focus

open System
open Jarvis

type Rule =
    | Forbid of string
    | Require of string

type Timebox =
    {
        Label: string
        Start: TimeOnly
        End: TimeOnly
        Rules: Rule list
    }

module Rule =

    let check (rule: Rule) (activity: Activity) =
        let str = (String.concat "  " activity).ToLower()
        match rule with
        | Forbid s -> str.Contains(s.ToLower()) |> not
        | Require s -> str.Contains(s.ToLower())