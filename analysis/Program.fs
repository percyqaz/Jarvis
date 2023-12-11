open System
open System.IO
open System.Globalization
open System.Collections.Generic
open System.Drawing

let LOGFILE = Path.Combine(Environment.GetFolderPath Environment.SpecialFolder.DesktopDirectory, "Source", "Jarvis", "logs", "activity_log.txt")
let LOG = File.ReadAllLines(LOGFILE)

// Activity categorisation

type Activity = string list

module Analysis =

    let private postFix2 (sep: string) (input: string) =
        let i = input.LastIndexOf(sep)
        if i > 0 then 
            let prefix = input.Substring(0, i)
            let suffix = input.Substring(i + sep.Length)
            Some prefix, suffix
        else None, input

    let private postFix (sep: string) (expectedSuffix: string) (input: string) =
        match postFix2 sep input with
        | Some prefix, suffix ->
            if suffix <> expectedSuffix then
                printfn "Expected suffix %A; got %A" expectedSuffix suffix
            Some prefix
        | None, suffix ->
            if suffix <> expectedSuffix then
                printfn "Expected suffix %A; got %A" expectedSuffix suffix
            None

    let identify (activity: string) : Activity =

        if activity.EndsWith "Microsoft Visual Studio" then
            match postFix " - " "Microsoft Visual Studio" activity with
            | Some project -> [project; "Visual Studio"]
            | None -> ["unknown"; "Visual Studio"]
        elif activity.EndsWith "Microsoft Visual Studio Debug Console" then
            ["Debug Console"; "Visual Studio"]

        elif activity.Contains "20 Minute Guided Meditation for Focus" then ["Meditation"]

        elif activity.EndsWith "Google Chrome" then
            match postFix " - " "Google Chrome" activity with
            | Some page ->

                if page.EndsWith "YouTube" then
                    match postFix " - " "YouTube" page with
                    | Some video ->
                        if video.Contains "#shorts" then ["#shorts"; "YouTube"; "Google Chrome"]
                        else [video; "YouTube"; "Google Chrome"]
                    | None -> ["unknown"; "YouTube"; "Google Chrome"]

                elif page.EndsWith "Gmail" then
                    match postFix " - " "Gmail" page with
                    | Some inbox -> [inbox; "Gmail"; "Google Chrome"]
                    | None -> ["unknown"; "Gmail"; "Google Chrome"]

                elif page.Contains " Watch anime online, " then
                    match postFix2 " | " page with
                    | None, _ -> ["unknown"; "Wcostream"; "Google Chrome"]
                    | Some cartoon, _ -> [cartoon; "Wcostream"; "Google Chrome"]

                else [page; "Google Chrome"]
            | None -> ["unknown"; "Google Chrome"]

        elif activity = "Azur Machine" then ["Azur Lane"]

        elif activity.EndsWith "Discord" then
            match postFix " - " "Discord" activity with
            | Some guild ->
                match postFix2 " | " guild with
                | Some channel, guild -> [channel; guild; "Discord"]
                | None, page -> [page; "Discord"]
            | None -> ["unknown"; "Discord"]

        elif activity.EndsWith "Notepad++" then
            match postFix " - " "Notepad++" activity with
            | Some file ->
                [file.Replace("*", ""); "Notepad++"]
            | None -> ["unknown"; "Notepad++"]

        elif activity = "SynergyDesk" then ["Work"]

        elif activity.Contains "Obsidian v1." then
            match postFix2 " - " activity with
            | Some obsidian, _ ->
                match postFix2 " - " obsidian with
                | Some page, vault -> [page; vault; "Obsidian"]
                | None, vault -> ["unknown"; vault; "Obsidian"]
            | None, _ -> ["unknown"; "Obsidian"]

        elif activity = "Cobalt" then ["Cobalt"; "Game"]
        elif activity = "Phasmophobia" then ["Phasmophobia"; "Game"]
        elif activity = "Interlude" then ["Interlude"; "Game"]
        elif activity.StartsWith "osu!" then ["osu!"; "Game"]
        elif activity.StartsWith "Terraria" then ["Terraria"; "Game"]

        elif activity = "" || activity = "Windows Default Lock Screen" || activity = "Program Manager" then ["Idle"]

        else [activity; "unknown"]

// Breakdown

module Breakdown =

    let breakdown = Dictionary<Activity, int>()

    let rec add_activity (activity: Activity) =
        match activity with
        | _ :: xs ->
            if breakdown.ContainsKey activity then
                breakdown.[activity] <- breakdown.[activity] + 10
            else
                breakdown.Add(activity, 10)
            add_activity xs
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

    let print_breakdown() =
        for k in breakdown.Keys |> Seq.sortBy(List.rev) do
            if breakdown.[k] > 60 then
                printfn "%-150s | %s" (String.replicate k.Length "  " + String.concat " > " (List.rev k)) (format_seconds breakdown.[k])

module Timeline =

    let colormap = Dictionary<Activity, Color>()

    let inline register_color a c = colormap.Add(a, c)

    register_color [] <| Color.FromArgb(127, 127, 127, 127)
    register_color ["Google Chrome"] Color.Yellow
    register_color ["YouTube"; "Google Chrome"] Color.Red
    register_color ["#shorts"; "YouTube"; "Google Chrome"] Color.Crimson
    register_color ["Wcostream"; "Google Chrome"] Color.Coral
    register_color ["Discord"] Color.MediumPurple
    register_color ["Azur Lane"] Color.Aquamarine
    register_color ["Work"] Color.Orange
    register_color ["Obsidian"] Color.RebeccaPurple
    register_color ["Visual Studio"] Color.Purple
    register_color ["Notepad++"] Color.White
    register_color ["Idle"] Color.Gray
    register_color ["Game"] Color.Blue
    register_color ["Meditation"] Color.Green
    register_color ["unknown"] Color.DarkGray

    let rec get_color (activity: Activity) =
        match activity with
        | _ :: xs ->
            if colormap.ContainsKey activity then colormap.[activity]
            else get_color xs
        | [] -> colormap.[[]]

    let color_data = Array.init 1440 (fun i -> Array.create 6 Color.Transparent)
    let insert_color (timestamp: DateTime) (activity: Activity) =
        let color = get_color activity
        let timecode = timestamp.Minute + timestamp.Hour * 60
        let data = color_data.[timecode]
        let mutable i = 0
        while data.[i] <> Color.Transparent && i < 5 do i <- i + 1
        data.[i] <- color

    let render_image(filename: string) =
        let bmp = new Bitmap(1440, 100, Imaging.PixelFormat.Format32bppArgb)
        bmp.MakeTransparent()
        for i, data in Array.indexed color_data do
            let A = data |> Array.sumBy (fun c -> float c.A) |> fun x -> x / 6.0 |> int
            let R = data |> Array.sumBy (fun c -> float c.R) |> fun x -> x / 6.0 |> int
            let G = data |> Array.sumBy (fun c -> float c.G) |> fun x -> x / 6.0 |> int
            let B = data |> Array.sumBy (fun c -> float c.B) |> fun x -> x / 6.0 |> int
            for y = 0 to 99 do
            bmp.SetPixel(i, y, Color.FromArgb(A, R, G, B))
        bmp.Save(filename, Imaging.ImageFormat.Png)

// Program

let date =
    let mutable date = Unchecked.defaultof<_>
    printf "Request date (dd/MM/yy) to report:"
    if DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, &date) then date
    else printfn "Using today's date."; DateTime.Today

for s in LOG do
    let split = s.Split("\t")
    let timestamp = split.[0]

    let title = split.[1]
    let timestamp = DateTime.ParseExact(timestamp, "dd-MM-yy HH:mm", CultureInfo.InvariantCulture)
    if date.Date = timestamp.Date then
        let a = Analysis.identify title
        Breakdown.add_activity a
        Timeline.insert_color timestamp a
Breakdown.print_breakdown()
Timeline.render_image("activity-"+date.ToString("dd-MM-yy")+".png")