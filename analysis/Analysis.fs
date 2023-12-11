namespace Jarvis

type Activity = string list

module Analysis =

    let private postFix2 (sep: string) (input: string) =
        let i = input.LastIndexOf(sep)
        if i >= 0 then 
            let prefix = input.Substring(0, i)
            let suffix = input.Substring(i + sep.Length)
            Some prefix, suffix
        else None, input

    let private postFix (sep: string) (expectedSuffix: string) (input: string) =
        match postFix2 sep input with
        | Some prefix, suffix ->
            //if suffix <> expectedSuffix then
            //    printfn "Expected suffix %A; got %A; original: %A" expectedSuffix suffix input
            Some prefix
        | None, suffix ->
            //if suffix <> expectedSuffix then
            //    printfn "Expected suffix %A; got %A; original: %A" expectedSuffix suffix input
            None

    let identify (activity: string) : Activity =

        if activity.EndsWith "Microsoft Visual Studio" then
            match postFix " - " "Microsoft Visual Studio" activity with
            | Some project -> [project; "Visual Studio"]
            | None -> ["unknown"; "Visual Studio"]
        elif activity.EndsWith "Microsoft Visual Studio Preview" then
            match postFix " - " "Microsoft Visual Studio Preview" activity with
            | Some project -> [project; "Visual Studio"]
            | None -> ["unknown"; "Visual Studio"]
        elif activity.EndsWith "Microsoft Visual Studio Debug Console" then
            ["Debug Console"; "Visual Studio"]
        elif activity.EndsWith "Visual Studio Code" then
            match postFix " - " "Visual Studio Code" activity with
            | Some project -> [project; "Visual Studio Code"]
            | None -> ["unknown"; "Visual Studio"]

        elif activity.Contains "20 Minute Guided Meditation for Focus" then ["Meditation"]

        elif activity.EndsWith "Google Chrome" || activity.EndsWith "Opera" then
            match fst (postFix2 " - " activity) with
            | Some page ->

                if page.EndsWith "YouTube" then
                    match postFix " - " "YouTube" page with
                    | Some video ->
                        if video.Contains "#shorts" then ["#shorts"; "YouTube"; "Web Browser"]
                        else [video; "YouTube"; "Web Browser"]
                    | None -> ["unknown"; "YouTube"; "Web Browser"]

                elif page.EndsWith "Gmail" then
                    match postFix " - " "Gmail" page with
                    | Some inbox -> [inbox; "Gmail"; "Web Browser"]
                    | None -> ["unknown"; "Gmail"; "Web Browser"]

                elif page.Contains " Watch anime online, " then
                    match postFix2 " | " page with
                    | None, _ -> ["unknown"; "Wcostream"; "Web Browser"]
                    | Some cartoon, _ -> [cartoon; "Wcostream"; "Web Browser"]

                else [page; "Web Browser"]
            | None -> ["unknown"; "Web Browser"]

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
        elif activity = "Zoom Meeting" then ["Tutoring"]
        elif activity.EndsWith "OneNote for Windows 10" then ["Tutoring"]
        
        elif activity.Contains "sensei@DESKTOP-4NGBUNJ:" then ["WSL"; "Terminal"]
        elif activity.Contains "MINGW64:" then ["Git Bash"; "Terminal"]
        elif activity.Contains "Windows Powershell" then ["Powershell"; "Terminal"]
        elif activity.EndsWith "cmd.exe" then ["Command Prompt"; "Terminal"]

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
        elif activity = "Lethal Company" then ["Lethal Company"; "Game"]
        elif activity = "Garry's Mod" then ["Garry's Mod"; "Game"]
        elif activity = "Counter-Strike 2" then ["Counter-Strike 2"; "Game"]
        elif activity.StartsWith "Factorio" then ["Factorio"; "Game"]
        elif activity.StartsWith "ELDEN RING" then ["ELDEN RING"; "Game"]
        elif activity.StartsWith "Minecraft" then ["Minecraft"; "Game"]
        elif activity.StartsWith "osu!" then ["osu!"; "Game"]
        elif activity.StartsWith "Terraria" then ["Terraria"; "Game"]

        elif activity = "" || activity = "Windows Default Lock Screen" || activity = "Program Manager" then ["Idle"]

        else [activity; "unknown"]