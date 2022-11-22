#NoEnv
#SingleInstance
#Warn
SendMode Input
SetCapsLockState, AlwaysOff
#Include activity.ahk
#Include drag-windows.ahk

; WIN-RIGHTCLICK TO ALWAYS-ON-TOP
LWin & RButton:: WinSet, AlwaysOnTop, , A

; WIN-DRAG WINDOWS
LWin & LButton::Gosub DRAGWINDOW

; TOGGLE 3RD MONITOR FOR WORK
LWin & F11::RunWait %ComSpec% /c MultiMonitorTool.exe /switch 3
LWin & F12::RunWait %ComSpec% /c MultiMonitorTool.exe /LoadConfig "monitorcfg"

; WINDOWS KEY NEEDS DOUBLE TAP TO WORK AS WINDOWS KEY
LWin::
	if (A_ThisHotkey = A_PriorHotkey && A_TimeSincePriorHotkey < 300)
	{
		Send {LWin down}
		keywait LWin
		Send {LWin up}
	}
return