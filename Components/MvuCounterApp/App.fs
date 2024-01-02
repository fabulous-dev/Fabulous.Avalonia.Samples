namespace MvuCounterApp

open Avalonia.Themes.Fluent
open Fabulous
open Fabulous.Avalonia

open type Fabulous.Avalonia.View

module App =
    let theme = FluentTheme()

    type Model =
        { Count: int; Step: int; TimerOn: bool }

    type Msg =
        | Increment
        | Decrement
        | Reset
        | SetStep of float
        | TimerToggled of bool
        | TimedTick

    type CmdMsg = | StartTimer

    let initModel = { Count = 0; Step = 1; TimerOn = false }

    let timerCmd () =
        async {
            do! Async.Sleep 200
            return TimedTick
        }
        |> Cmd.ofAsyncMsg

    let mapCmdMsgToCmd cmdMsg =
        match cmdMsg with
        | StartTimer -> timerCmd()

    let init () = initModel, []

    let update msg model =
        match msg with
        | Increment ->
            { model with
                Count = model.Count + model.Step },
            []
        | Decrement ->
            { model with
                Count = model.Count - model.Step },
            []
        | Reset -> initModel, []
        | SetStep n -> { model with Step = int(n + 0.5) }, []
        | TimerToggled on -> { model with TimerOn = on }, (if on then [ StartTimer ] else [])
        | TimedTick ->
            if model.TimerOn then
                { model with
                    Count = model.Count + model.Step },
                [ StartTimer ]
            else
                model, []

    let program = Program.ForComponent.statefulWithCmdMsg init update mapCmdMsgToCmd

    let view () =
        MvuComponent(program) {
            let! model = Mvu.State
#if MOBILE
            SingleViewApplication() {
                VStack() {
                    TextBlock($"%d{model.Count}")

                    Button("Increment", Increment)

                    Button("Decrement", Decrement)

                    (HStack() {
                        TextBlock("Timer")
                        ToggleSwitch(model.TimerOn, TimerToggled)
                    })
                        .centerHorizontal()

                    Slider(0.0, 10.0, double model.Step, SetStep)

                    TextBlock($"Step size: %d{model.Step}")

                    Button("Reset", Reset)
                }
            }
#else
            DesktopApplication(
                Window(
                    VStack() {
                        TextBlock($"%d{model.Count}")

                        Button("Increment", Increment)

                        Button("Decrement", Decrement)

                        (HStack() {
                            TextBlock("Timer")
                            ToggleSwitch(model.TimerOn, TimerToggled)
                        })
                            .centerHorizontal()

                        Slider(0.0, 10.0, double model.Step, SetStep)

                        TextBlock($"Step size: %d{model.Step}")

                        Button("Reset", Reset)
                    }
                )
            )

#endif
        }
