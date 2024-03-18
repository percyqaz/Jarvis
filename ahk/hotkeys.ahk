#NoEnv 
#SingleInstance Force
SendMode Input
SetWorkingDir %A_ScriptDir%
#NoTrayIcon

Menu(options, actions)
{
	menu_help = Options:`n
	for key, value in options
		menu_help = %menu_help%`n%key%`t--`t%value%
	ToolTip, %menu_help%
	
	Input, InputKey, L1 T3 M
	for key, value in actions
		if ( InputKey = key ) {
			gosub %value%
			ToolTip
			return
		}
		
	ToolTip, You pressed %InputKey% -- not recognised
		
	Sleep, 1000
	ToolTip
}

menu := { "I": "Interlude", "W": "Work", "O": "Obsidian" }
shortcuts := { "I": "interlude", "W": "work", "O": "obsidian" }

interlude:
submenu := {"D": "Local data", "C": "Changelog", "B": "Build output", "S": "Source code", "L": "Localisation (generated)"}
subshortcuts := {"D": "interlude_data", "C": "interlude_changelog", "B": "interlude_bin", "S": "interlude_source", "L": "interlude_generated_locale"}
Menu(submenu, subshortcuts)
return

interlude_source:
Run, C:\Users\percy\Desktop\Source\YAVSRG\Interlude
return

interlude_bin:
Run, C:\Users\percy\Desktop\Source\YAVSRG\Interlude\src\bin\Debug\net8.0
return

interlude_generated_locale:
Run, C:\Users\percy\Desktop\Source\YAVSRG\Interlude\src\bin\Debug\net8.0\Locale\en_GB.txt
return

interlude_data:
Run, C:\Interlude\dev
return

interlude_changelog:
Run, C:\Users\percy\Desktop\Source\YAVSRG\Interlude\docs\changelog.md
return

work:
Run, C:\Users\percy\Desktop\work
return

obsidian:
Run, C:\Users\percy\AppData\Local\Obsidian\Obsidian.exe
return

^`::Menu(menu, shortcuts)