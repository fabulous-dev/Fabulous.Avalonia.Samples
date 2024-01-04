namespace GitHubClient

open Fabulous
open GitHubClient.Models
open Fabulous.Avalonia

open type Fabulous.Avalonia.View

module FavoritesPage =
    type Msg =
        | FollowersLoaded of Follower array
        | FollowersNotFound of GitHubError

    type CmdMsg = GetFollowers of name: string

    type Model =
        { UserName: string
          Followers: Follower array }

    let init () = { UserName = ""; Followers = [||] }, []

    let getFollowers userName =
        task {
            let! response = GitHubService.getFollowers userName 1

            match response with
            | Ok followers -> return FollowersLoaded followers
            | Error error -> return FollowersNotFound error
        }

    let mapCmdMsgToCmd cmdMsg =
        match cmdMsg with
        | GetFollowers userName -> Cmd.ofTaskMsg(getFollowers userName)

    let update msg model =
        match msg with
        | FollowersLoaded followers -> { model with Followers = followers }, []

        | FollowersNotFound _ -> model, []

    let view _ =
        (Grid() { TextBlock("Favorites") }).center()
