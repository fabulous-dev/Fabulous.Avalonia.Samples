namespace DrawingApp.iOS

open System
open Foundation
open Fabulous.Avalonia
open DrawingApp

[<Register(nameof SceneDelegate)>]
type SceneDelegate() =
    inherit FabSceneDelegate()

    override this.CreateApp() =
        let app = Program.startComponentApplication App.view
        app.Styles.Add(App.theme)
        app
