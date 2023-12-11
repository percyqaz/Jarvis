namespace Jarvis

open System
open System.Collections.Generic
open System.Drawing

type Timeline = Color array array

module Timeline =

    let private colormap = Dictionary<Activity, Color>()

    let inline private register_color a c = colormap.Add(a, c)

    register_color [] <| Color.FromArgb(127, 127, 127, 127)
    register_color ["Web Browser"] Color.Yellow
    register_color ["YouTube"; "Web Browser"] Color.Red
    register_color ["#shorts"; "YouTube"; "Web Browser"] Color.Crimson
    register_color ["Wcostream"; "Web Browser"] Color.Coral
    register_color ["Discord"] Color.MediumPurple
    register_color ["Azur Lane"] Color.Aquamarine
    register_color ["Work"] Color.Green
    register_color ["Obsidian"] Color.RebeccaPurple
    register_color ["Visual Studio"] Color.Purple
    register_color ["Notepad++"] Color.White
    register_color ["Idle"] Color.Gray
    register_color ["Game"] Color.Blue
    register_color ["Meditation"] Color.Green
    register_color ["unknown"] Color.DarkGray

    let rec private get_color (activity: Activity) =
        match activity with
        | _ :: xs ->
            if colormap.ContainsKey activity then colormap.[activity]
            else get_color xs
        | [] -> colormap.[[]]

    let empty() = Array.init 1440 (fun _ -> Array.create 6 Color.Transparent)

    let insert_color (timeline: Timeline) (timestamp: DateTime) (activity: Activity) =
        let color = get_color activity
        let timecode = timestamp.Minute + timestamp.Hour * 60
        let data = timeline.[timecode]
        let mutable i = 0
        while data.[i] <> Color.Transparent && i < 5 do i <- i + 1
        data.[i] <- color

    let render (timeline: Timeline) (filename: string) =
        let bmp = new Bitmap(1440, 100, Imaging.PixelFormat.Format32bppArgb)
        bmp.MakeTransparent()
        for i, data in Array.indexed timeline do
            let A = data |> Array.sumBy (fun c -> float c.A) |> fun x -> x / 6.0 |> int
            let R = data |> Array.sumBy (fun c -> float c.R) |> fun x -> x / 6.0 |> int
            let G = data |> Array.sumBy (fun c -> float c.G) |> fun x -> x / 6.0 |> int
            let B = data |> Array.sumBy (fun c -> float c.B) |> fun x -> x / 6.0 |> int
            for y = 0 to 99 do
            bmp.SetPixel(i, y, Color.FromArgb(A, R, G, B))
        bmp.Save(filename, Imaging.ImageFormat.Png)
