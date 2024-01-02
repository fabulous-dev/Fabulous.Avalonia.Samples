namespace CounterApp.iOS

open System
open Fabulous
open Foundation
open Fabulous.Avalonia
open CounterApp

[<Register(nameof SceneDelegate)>]
type SceneDelegate() =
    inherit FabSceneDelegate()

    override this.CreateApp() =
        let app = Program.startComponentApplication(App.view())
        app.Styles.Add(App.theme)
        app
