namespace GitHubClient

open System.Diagnostics
open Avalonia.Themes.Fluent
open Fabulous
open Fabulous.Avalonia
open GitHubClient.Models

open type Fabulous.Avalonia.View


module App =
    type Msg =
        | UserNameChanged of string
        | SearchClicked
        | UserInfoLoaded of User
        | UserInfoNotFound of GitHubError
        | FollowersLoaded of Follower array
        | FollowersNotFound of GitHubError

    type CmdMsg =
        | GetUserInfo of name: string
        | GetFollowers of name: string

    type Model = { UserName: string; Followers: Follower  array }

    let init () = { UserName = ""; Followers = [||] }, []

    let getUserInfo userName =
        task {
            let! response = GitHubService.getUserInfo userName

            match response with
            | Ok user -> return UserInfoLoaded user
            | Error error -> return UserInfoNotFound error
        }
        
    let getFollowers userName =
        task {
            let! response = GitHubService.getFollowers userName 1

            match response with
            | Ok followers -> return FollowersLoaded followers
            | Error error -> return FollowersNotFound error
        }

    let mapCmdMsgToCmd cmdMsg =
        match cmdMsg with
        | GetUserInfo userName -> Cmd.ofTaskMsg(getUserInfo userName)
        | GetFollowers userName -> Cmd.ofTaskMsg(getFollowers userName)

    let update msg model =
        match msg with
        | UserNameChanged userName -> { model with UserName = userName }, []

        | SearchClicked -> model, [ GetUserInfo model.UserName ]

        | UserInfoLoaded user -> model, [ GetFollowers user.login ]

        | UserInfoNotFound _ -> model, []
        
        | FollowersLoaded followers -> { model with Followers = followers }, []
        
        | FollowersNotFound _ -> model, []

    let view model =
        Grid() {
            (VStack() {
                Image(ImageSource.fromString("avares://GitHubClient/Assets/github-icon.png"))
                    .size(100., 100.)

                TextBox(model.UserName, UserNameChanged)
                Button("Search", SearchClicked)
                
                UniformGrid(cols = 2, rows = 37) {
                    for i in 0 .. model.Followers.Length - 1 do
                        TextBlock(model.Followers[i].login)
                            .gridRow(i / 2)
                    }
            }).centerHorizontal()
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
