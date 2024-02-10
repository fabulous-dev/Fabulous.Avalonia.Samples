namespace GitHubClient.Desktop

open System
open Avalonia
open Fabulous.Avalonia
open GitHubClient

module Program =

    [<CompiledName "BuildAvaloniaApp">]
    let buildAvaloniaApp () = App.create().UsePlatformDetect()

    [<EntryPoint; STAThread>]
    let main argv =
        buildAvaloniaApp().StartWithClassicDesktopLifetime(argv)
