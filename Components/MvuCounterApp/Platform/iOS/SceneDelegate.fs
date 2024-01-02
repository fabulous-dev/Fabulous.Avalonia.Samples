namespace MvuCounterApp.iOS

open System
open Foundation
open Fabulous.Avalonia
open MvuCounterApp

[<Register(nameof SceneDelegate)>]
type SceneDelegate() =
    inherit FabSceneDelegate()

    override this.CreateApp() =
        let app = Program.startMvuComponentApplication(App.program, App.view())
        app.Styles.Add(App.theme)
        app
