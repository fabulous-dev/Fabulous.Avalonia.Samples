namespace GameOfLife.Desktop

open System
open Avalonia

open Avalonia.Themes.Fluent
open GameOfLife
open Fabulous.Avalonia

module Program =

    [<CompiledName "BuildAvaloniaApp">]
    let buildAvaloniaApp () = App.create().UsePlatformDetect()

    [<EntryPoint; STAThread>]
    let main argv =
        buildAvaloniaApp().StartWithClassicDesktopLifetime(argv)
