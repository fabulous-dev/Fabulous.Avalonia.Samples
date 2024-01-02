namespace CounterApp.Desktop

open System
open Avalonia
open Fabulous
open Fabulous.Avalonia
open CounterApp

module Program =

    [<CompiledName "BuildAvaloniaApp">]
    let buildAvaloniaApp () =
        AppBuilder
            .Configure(fun () ->
                let app = Program.startComponentApplication(App.view())
                app.Styles.Add(App.theme)
                app)
            .LogToTrace(areas = Array.empty)
            .UsePlatformDetect()

    [<EntryPoint; STAThread>]
    let main argv =
        buildAvaloniaApp().StartWithClassicDesktopLifetime(argv)
