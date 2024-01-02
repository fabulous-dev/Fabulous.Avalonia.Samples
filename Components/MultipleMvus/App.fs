namespace MultipleMvus

open Avalonia.Themes.Fluent
open Fabulous
open Fabulous.Avalonia

open type Fabulous.Avalonia.View

module Counter =
    type Model = { Count: int }

    type Msg =
        | Increment
        | Decrement

    let init () = { Count = 0 }

    let update msg model =
        match msg with
        | Increment -> { Count = model.Count + 1 }
        | Decrement -> { Count = model.Count - 1 }

    let program = Program.ForComponent.stateful init update

module Form =
    type Model = { FirstName: string; LastName: string }

    type Msg =
        | FirstNameChanged of string
        | LastNameChanged of string

    let init () = { FirstName = ""; LastName = "" }

    let update msg model =
        match msg with
        | FirstNameChanged s -> { model with FirstName = s }
        | LastNameChanged s -> { model with LastName = s }

    let program = Program.ForComponent.stateful init update

module App =
    let theme = FluentTheme()

    let view () =
#if MOBILE
        SingleViewApplication() {
            (VStack(spacing = 25.) {
                Label("App")

                MvuComponent(Counter.program) {
                    let! model = Mvu.State

                    VStack() {
                        Label($"Count = {model.Count}")
                        Button("Increment", Counter.Increment)
                        Button("Decrement", Counter.Decrement)
                    }
                }

                MvuComponent(Form.program) {
                    let! model = Mvu.State

                    VStack() {
                        Label($"Hello {model.FirstName} {model.LastName}")
                        TextBox(model.FirstName, Form.FirstNameChanged)
                        TextBox(model.LastName, Form.LastNameChanged)
                    }
                }
            })
                .width(250.)
                .center()
        }
#else
        DesktopApplication(
            Window() {
                (VStack(spacing = 25.) {
                    Label("App")

                    MvuComponent(Counter.program) {
                        let! model = Mvu.State

                        VStack() {
                            Label($"Count = {model.Count}")
                            Button("Increment", Counter.Increment)
                            Button("Decrement", Counter.Decrement)
                        }
                    }

                    MvuComponent(Form.program) {
                        let! model = Mvu.State

                        VStack() {
                            Label($"Hello {model.FirstName} {model.LastName}")
                            TextBox(model.FirstName, Form.FirstNameChanged)
                            TextBox(model.LastName, Form.LastNameChanged)
                        }
                    }
                })
                    .width(250.)
                    .center()
            }
        )
#endif
