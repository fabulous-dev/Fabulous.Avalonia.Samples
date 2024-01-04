namespace GitHubClient

open System.Diagnostics
open System.IO
open Avalonia.Media.Imaging
open Avalonia.Themes.Fluent
open Fabulous
open Fabulous.Avalonia
open System.Net
open System
open System.Net.Http

open System.Text.Json

open type Fabulous.Avalonia.View

module Models =
    type RemoteData<'e, 't> =
        | NotAsked
        | Loading
        | Content of 't
        | Failure of 'e

    type User =
        { login: string
          avatar_url: string
          name: string option
          location: string option
          bio: string option
          public_repos: int
          public_gists: int
          html_url: string
          following: int
          followers: int
          created_at: DateTime }

open Models

module URlConstants =

    [<Literal>]
    let githubBaseUrl = "https://api.github.com/users/"

type GitHubError = | Non200Response

module GitHubService =
    let private fetchWitHeader (urlString: string) =
        let client = new HttpClient()
        client.DefaultRequestHeaders.Add("User-Agent", "F# App")
        client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json")
        client.GetAsync(urlString)

    let private fetchGetByteArrayAsyncWitHeader (urlString: string) =
        let client = new HttpClient()
        client.DefaultRequestHeaders.Add("User-Agent", "F# App")
        client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json")
        client.GetByteArrayAsync(urlString)

    let getUserInfo (userName: string) =
        let urlString = $"{URlConstants.githubBaseUrl}%s{userName}"

        task {
            use! response = fetchWitHeader urlString
            response.EnsureSuccessStatusCode |> ignore

            let! followers = response.Content.ReadAsStringAsync()
            let deserialized = JsonSerializer.Deserialize<User>(followers)

            return
                match response.StatusCode with
                | HttpStatusCode.OK -> Ok deserialized
                | _ -> Error Non200Response
        }

    let getProfileImage (urlString: string) =
        async {
            let client = new HttpClient()
            let! response = client.GetAsync(urlString) |> Async.AwaitTask
            response.EnsureSuccessStatusCode() |> ignore
            let! data = response.Content.ReadAsByteArrayAsync() |> Async.AwaitTask

            return
                match response.StatusCode with
                | HttpStatusCode.OK -> Ok(new Bitmap(new MemoryStream(data)))
                | _ -> Error Non200Response
        }


module App =
    type Msg =
        | UserNameChanged of string
        | SearchClicked
        | UserInfoLoaded of User
        | UserInfoNotFound of GitHubError
        | ProfileImageLoaded of Bitmap
        | ProfileImageNotFound of GitHubError
        | LoadingProgress of float

    type CmdMsg =
        | GetUserInfo of name: string
        | GetProfileImage of url: string

    type Model =
        { UserName: string
          UserInfo: RemoteData<string, User>
          ProfileImage: RemoteData<string, Bitmap> }

    let init () =
        { UserName = ""
          UserInfo = RemoteData.NotAsked
          ProfileImage = RemoteData.NotAsked },
        []

    let getUserInfo userName =
        task {
            let! response = GitHubService.getUserInfo userName

            match response with
            | Ok user -> return UserInfoLoaded user
            | Error error -> return UserInfoNotFound error
        }

    let getProfileImage url =
        task {
            let! response = GitHubService.getProfileImage url

            match response with
            | Ok image -> return ProfileImageLoaded image
            | Error error -> return ProfileImageNotFound error
        }

    let mapCmdMsgToCmd cmdMsg =
        match cmdMsg with
        | GetUserInfo userName -> Cmd.ofTaskMsg(getUserInfo userName)
        | GetProfileImage url -> Cmd.ofTaskMsg(getProfileImage url)

    let update msg model =
        match msg with
        | UserNameChanged userName -> { model with UserName = userName }, []

        | SearchClicked ->
            { model with
                UserInfo = RemoteData.Loading },
            [ GetUserInfo model.UserName ]

        | UserInfoLoaded user ->
            { model with
                UserInfo = RemoteData.Content(user) },
            [ GetProfileImage(user.avatar_url) ]

        | UserInfoNotFound _ ->
            { model with
                UserInfo = RemoteData.Failure("User not found!") },
            []

        | ProfileImageLoaded image ->
            { model with
                ProfileImage = RemoteData.Content(image) },
            []

        | ProfileImageNotFound _ ->
            { model with
                ProfileImage = RemoteData.Failure("Profile image not found!") },
            []

        | LoadingProgress _ -> model, []

    let view model =
        Grid() {
            (VStack() {
                Image(ImageSource.fromString("avares://GitHubClient/Assets/github-icon.png"))
                    .size(100., 100.)

                TextBox(model.UserName, UserNameChanged)
                Button("Search", SearchClicked)

                match model.UserInfo with
                | NotAsked -> ()
                | Loading -> ProgressBar(0., 1., 0.5, LoadingProgress)
                | Content user ->
                    (VStack() {
                        match model.ProfileImage with
                        | NotAsked -> ()
                        | Loading -> ()
                        | Content bitmap -> Image(bitmap).size(24., 24.)
                        | Failure _ ->
                            Image(ImageSource.fromString("avares://GitHubClient/Assets/github-icon.png"))
                                .size(24., 24.)

                        TextBlock(user.login)

                        if user.name.IsSome then
                            TextBlock(user.name.Value)

                        if user.location.IsSome then
                            TextBlock(user.location.Value)

                            if user.bio.IsSome then
                                TextBlock(user.bio.Value)

                            TextBlock($"Public repos: {user.public_repos}")

                            TextBlock($"Public gists: {user.public_gists}")

                            TextBlock($"Followers: {user.followers}")

                            TextBlock($"Following: {user.following}")

                            TextBlock($"Created at: {user.created_at}")

                            TextBlock($"Profile: {user.html_url}")
                    })
                        .centerHorizontal()
                | Failure s -> TextBlock(s)
            })
                .centerHorizontal()
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
