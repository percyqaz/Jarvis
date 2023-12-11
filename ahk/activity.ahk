#NoEnv
#SingleInstance
#Warn
SendMode Input
SetWorkingDir %A_ScriptDir%

interval = 10 ; Number of seconds to wait

loop {
    sleep, % interval*1000
	WinGetActiveTitle, WinTitle
	FormatTime, CurrentDateTime,, dd-MM-yy HH:mm
	FileAppend, %CurrentDateTime%`t%WinTitle%`n, ../logs/activity_log.txt
}