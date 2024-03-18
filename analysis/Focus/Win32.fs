namespace Jarvis.Focus

open System
open System.Text
open System.Runtime.InteropServices

module Win32 =

    [<DllImport("user32.dll")>]
    extern IntPtr GetForegroundWindow()

    [<DllImport("user32.dll")>]
    extern int32 GetWindowText(IntPtr hWnd, StringBuilder text, int count)
    
    [<DllImport("user32.dll")>]
    extern bool LockWorkStation()

module InteropHelpers =

    let get_window_title() : string =

        let BUFFER_SIZE = 256
        let buff = new StringBuilder(BUFFER_SIZE)
        let handle = Win32.GetForegroundWindow()
        if Win32.GetWindowText(handle, buff, BUFFER_SIZE) > 0 then buff.ToString() else ""

    let lock_pc() = Win32.LockWorkStation()