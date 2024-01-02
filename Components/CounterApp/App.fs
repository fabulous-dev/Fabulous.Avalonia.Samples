namespace CounterApp

open Avalonia.Themes.Fluent
open Fabulous
open Fabulous.Avalonia

open type Fabulous.Avalonia.View

module App =
    let theme = FluentTheme()

    let view () =
        Component() {
            let! count = State<int>(fun _ -> 0)
#if MOBILE
            SingleViewApplication() {
                (VStack() {
                    TextBlock($"%d{count.Current}")
                    Button("Decrement", (fun () -> count.Set(count.Current - 1)))
                    Button("Increment", (fun () -> count.Set(count.Current + 1)))
                    Button("Reset", (fun () -> count.Set(0)))
                })
                    .center()
            }
#else
            DesktopApplication(
                Window(
                    (VStack() {
                        TextBlock($"%d{count.Current}")
                        Button("Decrement", (fun () -> count.Set(count.Current - 1)))
                        Button("Increment", (fun () -> count.Set(count.Current + 1)))
                        Button("Reset", (fun () -> count.Set(0)))
                    })
                        .center()
                )
            )
#endif
        }
