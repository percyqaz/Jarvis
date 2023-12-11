#NoEnv  ; Recommended for performance and compatibility with future AutoHotkey releases.
; #Warn  ; Enable warnings to assist with detecting common errors.
SendMode Input  ; Recommended for new scripts due to its superior speed and reliability.
SetWorkingDir %A_ScriptDir%  ; Ensures a consistent starting directory.

WinGetPosEx(hWindow,ByRef X="",ByRef Y="",ByRef Width="",ByRef Height="",ByRef Offset_X="",ByRef Offset_Y="")
{
    Static Dummy5693
          ,RECTPlus
          ,S_OK:=0x0
          ,DWMWA_EXTENDED_FRAME_BOUNDS:=9

    ;-- Workaround for AutoHotkey Basic
    PtrType:=(A_PtrSize=8) ? "Ptr":"UInt"

    ;-- Get the window's dimensions
    ;   Note: Only the first 16 bytes of the RECTPlus structure are used by the
    ;   DwmGetWindowAttribute and GetWindowRect functions.
    VarSetCapacity(RECTPlus,24,0)
    DWMRC:=DllCall("dwmapi\DwmGetWindowAttribute"
        ,PtrType,hWindow                                ;-- hwnd
        ,"UInt",DWMWA_EXTENDED_FRAME_BOUNDS             ;-- dwAttribute
        ,PtrType,&RECTPlus                              ;-- pvAttribute
        ,"UInt",16)                                     ;-- cbAttribute

    if (DWMRC<>S_OK)
        {
        if ErrorLevel in -3,-4  ;-- Dll or function not found (older than Vista)
            {
            ;-- Do nothing else (for now)
            }
         else
            outputdebug,
               (ltrim join`s
                Function: %A_ThisFunc% -
                Unknown error calling "dwmapi\DwmGetWindowAttribute".
                RC=%DWMRC%,
                ErrorLevel=%ErrorLevel%,
                A_LastError=%A_LastError%.
                "GetWindowRect" used instead.
               )

        ;-- Collect the position and size from "GetWindowRect"
        DllCall("GetWindowRect",PtrType,hWindow,PtrType,&RECTPlus)
        }

    ;-- Populate the output variables
    X:=Left :=NumGet(RECTPlus,0,"Int")
    Y:=Top  :=NumGet(RECTPlus,4,"Int")
    Right   :=NumGet(RECTPlus,8,"Int")
    Bottom  :=NumGet(RECTPlus,12,"Int")
    Width   :=Right-Left
    Height  :=Bottom-Top
    OffSet_X:=0
    OffSet_Y:=0

    ;-- If DWM is not used (older than Vista or DWM not enabled), we're done
    if (DWMRC<>S_OK)
        Return &RECTPlus

    ;-- Collect dimensions via GetWindowRect
    VarSetCapacity(RECT,16,0)
    DllCall("GetWindowRect",PtrType,hWindow,PtrType,&RECT)
    GWR_Width :=NumGet(RECT,8,"Int")-NumGet(RECT,0,"Int")
        ;-- Right minus Left
    GWR_Height:=NumGet(RECT,12,"Int")-NumGet(RECT,4,"Int")
        ;-- Bottom minus Top

    ;-- Calculate offsets and update output variables
    NumPut(Offset_X:=(Width-GWR_Width)//2,RECTPlus,16,"Int")
    NumPut(Offset_Y:=(Height-GWR_Height)//2,RECTPlus,20,"Int")
    Return &RECTPlus
}
WindowBoundingBox(MonitorIndex, Index, ByRef X = "", ByRef Y = "", ByRef X2 = "", ByRef Y2 = "")
{
    SysGet Mon, MonitorWorkArea, %MonitorIndex%

    MonWidth := % MonRight - MonLeft
    MonDiv1 := % MonLeft + MonWidth * 0.3
    MonDiv2 := % MonRight - MonWidth * 0.3
    MonDiv := % MonLeft + MonWidth * 0.5
    MonHalfway := % MonTop + (MonBottom - MonTop) * 0.5

    if MonWidth > 2000
    {
        ; Split bottom half of widescreen into 3 panels
        if Index = 1
        {
            X := MonLeft
            Y := MonHalfway
            X2 := MonDiv1
            Y2 := MonBottom
            return true
        }
        else if Index = 2
        {
            X := MonDiv1
            Y := MonHalfway
            X2 := MonDiv2
            Y2 := MonBottom
            return true
        }
        else if Index = 3
        {
            X := MonDiv2
            Y := MonHalfway
            X2 := MonRight
            Y2 := MonBottom
            return true
        }
        ; Split top half of widescreen into 3 panels
        else if Index = 4
        {
            X := MonLeft
            Y := MonTop
            X2 := MonDiv1
            Y2 := MonHalfway
            return true
        }
        else if Index = 5
        {
            X := MonDiv1
            Y := MonTop
            X2 := MonDiv2
            Y2 := MonHalfway
            return true
        }
        else if Index = 6
        {
            X := MonDiv2
            Y := MonTop
            X2 := MonRight
            Y2 := MonHalfway
            return true
        }
        ; Split widescreen into 3 columns
        else if Index = 7
        {
            X := MonLeft
            Y := MonTop
            X2 := MonDiv1
            Y2 := MonBottom
            return true
        }
        else if Index = 8
        {
            X := MonDiv1
            Y := MonTop
            X2 := MonDiv2
            Y2 := MonBottom
            return true
        }
        else if Index = 9
        {
            X := MonDiv2
            Y := MonTop
            X2 := MonRight
            Y2 := MonBottom
            return true
        }
        ; Large-fit windows for widescreen
        else if Index = 10
        {
            X := MonLeft
            Y := MonTop
            X2 := MonDiv2
            Y2 := MonBottom
            return true
        }
        else if Index = 11
        {
            X := MonDiv1
            Y := MonTop
            X2 := MonRight
            Y2 := MonBottom
            return false
        }
    }
    else {
        ; Split MainMon into two columns
        if Index = 1
        {
            X := MonLeft
            Y := MonTop
            X2 := MonDiv
            Y2 := MonBottom
            return true
        }
        else if Index = 2
        {
            X := MonDiv
            Y := MonTop
            X2 := MonRight
            Y2 := MonBottom
            return false
        }
    }
}

DRAGWINDOW:
    CoordMode, Mouse  ; Switch to screen/absolute coordinates.
    MouseGetPos, EWD_MouseStartX, EWD_MouseStartY, EWD_MouseWin
    WinGetPos, EWD_OriginalPosX, EWD_OriginalPosY,,, ahk_id %EWD_MouseWin%
    WinGet, EWD_WinState, MinMax, ahk_id %EWD_MouseWin%
    if EWD_WinState = 0  ; Only if the window isn't maximized
        SetTimer, EWD_WatchMouse, 10 ; Track the mouse as the user drags it.
return

EWD_WatchMouse:
    CoordMode, Mouse
    MouseGetPos, EWD_MouseX, EWD_MouseY
    WinGetPos, EWD_WinX, EWD_WinY,,, ahk_id %EWD_MouseWin%
    SetWinDelay, -1   ; Makes the below move faster/smoother.
    WinMove, ahk_id %EWD_MouseWin%,, EWD_WinX + EWD_MouseX - EWD_MouseStartX, EWD_WinY + EWD_MouseY - EWD_MouseStartY
    EWD_MouseStartX := EWD_MouseX  ; Update for the next timer-call to this subroutine.
    EWD_MouseStartY := EWD_MouseY

    GetKeyState, EWD_LButtonState, LButton, P
    if EWD_LButtonState = U  ; Button has been released, so drag is complete.
    {
        BoundingBoxMargin := 150
        SysGet, Monitors, MonitorCount
        M_Index := 1
        While (M_Index <= Monitors)
        {
            BBIndex := 1
            HasNext := true
            While (HasNext) {
                HasNext := WindowBoundingBox(M_Index, BBIndex, BBLeft, BBTop, BBRight, BBBottom)
                if (EWD_MouseX > BBLeft + BoundingBoxMargin) and (EWD_MouseX < BBRight - BoundingBoxMargin) and (EWD_MouseY > BBTop + BoundingBoxMargin) and (EWD_MouseY < BBBottom - BoundingBoxMargin)
                {
                    WinGetPosEx(EWD_MouseWin, ,,,, OffsetX, OffsetY)
                    WinMove, ahk_id %EWD_MouseWin%,, % BBLeft + OffsetX, %BBTop%, % BBRight - BBLeft - OffsetX * 2, % BBBottom - BBTop - OffsetY * 2
                    break
                }
                BBIndex := BBIndex + 1
            }
            M_Index := M_Index + 1
        }
        SetTimer, EWD_WatchMouse, off
        return
    }
    GetKeyState, EWD_EscapeState, Escape, P
    if EWD_EscapeState = D  ; Escape has been pressed, so drag is cancelled.
    {
        SetTimer, EWD_WatchMouse, off
        WinMove, ahk_id %EWD_MouseWin%,, %EWD_OriginalPosX%, %EWD_OriginalPosY%
        return
    }
return