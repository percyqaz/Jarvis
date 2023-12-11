open System
open System.IO
open System.Globalization
open System.Linq
open Jarvis

let LOGFILE = Path.Combine(Environment.GetFolderPath Environment.SpecialFolder.DesktopDirectory, "Source", "Jarvis", "logs", "activity_log.txt")
let RAW_LOG = File.ReadAllLines(LOGFILE)

type LogEntry =
    {
        Timestamp: DateTime
        RawActivity: string
        Activity: Activity
    }

let LOG =
    printfn "Processing log file ..."
    RAW_LOG 
    |> Array.map (fun s ->
        let split = s.Split("\t")
        let timestamp = DateTime.ParseExact(split.[0], "dd-MM-yy HH:mm", CultureInfo.InvariantCulture)
        let raw_activity = split.[1]
        let activity = Analysis.identify raw_activity
        { 
            Timestamp = timestamp
            RawActivity = raw_activity
            Activity = activity
        }
    )

// Program

while true do
    printfn "1. Search by date\n2. Search by name\n"
    match Console.ReadKey().Key with
    | ConsoleKey.D1 ->
        let date =
            let mutable date = Unchecked.defaultof<_>
            printf "Request date (dd/MM/yy) to report:"
            if DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, &date) then date
            else printfn "Using today's date."; DateTime.Today

        let today =
            LOG
            |> Array.where (fun x -> x.Timestamp.Date = date)

        let breakdown = Breakdown.create (today |> Array.map _.Activity)
        Breakdown.print breakdown
        Console.ReadLine() |> ignore
    | ConsoleKey.D2 ->
        printf "Enter your search term: "
        let query = Console.ReadLine()
        let matches = LOG |> Array.where(fun x -> x.Activity.Any(fun y -> y.Contains(query)))
        for day in matches.GroupBy(fun x -> x.Timestamp.Date) do
            printfn "\n==== %s ====" (day.Key.ToShortDateString())
            Breakdown.create (day.Select(fun x -> x.Activity).ToArray())
            |> Breakdown.print

        printfn "----\nTotal time: %s" (Breakdown.format_seconds (10 * matches.Length))
        Console.ReadLine() |> ignore
    | _ -> ()
    Console.Clear()

//let timeline = Timeline.empty()

//for a in today do
//    Timeline.insert_color timeline a.Timestamp a.Activity

//Timeline.render timeline (Path.Combine(Environment.GetFolderPath Environment.SpecialFolder.DesktopDirectory, "Source", "Jarvis", "graphs", "activity-"+date.ToString("dd-MM-yy")+".png"))