namespace GitHubClient

open System.Diagnostics
open Avalonia.Themes.Fluent
open Fabulous
open Fabulous.Avalonia
open Models

open type Fabulous.Avalonia.View


module App =
    type Msg =
        | UserNameChanged of string
        | SearchClicked
        | UserLoaded of User
        | UserNotFound of GitHubError

    type CmdMsg = FetchUser of string

    type Model = { UserName: string }

    let init () = { UserName = "" }, []

    let getFollowers userName =
        task {
            let! response = GitHubService.getUserInfo userName

            match response with
            | Ok user -> return UserLoaded user
            | Error error -> return UserNotFound error
        }

    let mapCmdMsgToCmd cmdMsg =
        match cmdMsg with
        | FetchUser userName -> Cmd.ofTaskMsg(getFollowers userName)

    let update msg model =
        match msg with
        | UserNameChanged userName -> { UserName = userName }, []

        | SearchClicked -> model, [ FetchUser model.UserName ]

        | UserLoaded user -> model, []

        | UserNotFound error -> model, []

    let view model =
        Grid() {
            VStack() {
                Image(ImageSource.fromString("avares://GitHubClient/Assets/github-icon.png"))
                    .size(100., 100.)

                TextBox(model.UserName, UserNameChanged)
                Button("Search", SearchClicked)
            }
        }

#if MOBILE
    let app model = SingleViewApplication(view model)
#else
    let app model = DesktopApplication(Window(view model))
#endif

    let theme = FluentTheme()

    let program =
        Program.statefulWithCmdMsg init update app mapCmdMsgToCmd
        |> Program.withTrace(fun (format, args) -> Debug.WriteLine(format, box args))
        |> Program.withExceptionHandler(fun ex ->
#if DEBUG
            printfn $"Exception: %s{ex.ToString()}"
            false
#else
            true
#endif
        )
