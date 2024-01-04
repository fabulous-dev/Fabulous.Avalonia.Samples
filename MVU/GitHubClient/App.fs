namespace GitHubClient

open System.Diagnostics
open Avalonia.Controls
open Avalonia.Themes.Fluent
open Fabulous
open Fabulous.Avalonia
open GitHubClient

open type Fabulous.Avalonia.View

module App =
    type Msg =
        | SearchPageMsg of SearchPage.Msg
        | FavoritesPageMsg of FavoritesPage.Msg

    type SubpageCmdMsg =
        | SearchCmdMsgs of SearchPage.CmdMsg list
        | FavoritesCmdMsgs of FavoritesPage.CmdMsg list

    type Model =
        { SearchModel: SearchPage.Model
          FavoritesModel: FavoritesPage.Model }

    type CmdMsg =
        | NewMsg of Msg
        | SubpageCmdMsgs of SubpageCmdMsg list

    let init () =
        let search, searchCmdMsgs = SearchPage.init()
        let favorites, favoritesCmdMsgs = FavoritesPage.init()

        { SearchModel = search
          FavoritesModel = favorites },
        [ SubpageCmdMsgs searchCmdMsgs; SubpageCmdMsgs favoritesCmdMsgs ]

    let mapCmdMsgToCmd cmdMsg =
        match cmdMsg with
        | NewMsg msg -> Cmd.ofMsg msg
        | SubpageCmdMsgs cmdMsgs ->
            let mapSubpageCmdMsg (cmdMsg: SubpageCmdMsg) =
                let map (mapCmdMsgFn: 'subCmdMsg -> Cmd<'subMsg>) (mapFn: 'subMsg -> 'msg) (subCmdMsgs: 'subCmdMsg list) =
                    subCmdMsgs
                    |> List.map(fun c ->
                        let cmd = mapCmdMsgFn c
                        Cmd.map mapFn cmd)

                match cmdMsg with
                | SearchCmdMsgs subCmdMsgs -> map SearchPage.mapCmdMsgToCmd SearchPageMsg subCmdMsgs
                | FavoritesCmdMsgs subCmdMsgs -> map FavoritesPage.mapCmdMsgToCmd FavoritesPageMsg subCmdMsgs

            cmdMsgs |> List.map mapSubpageCmdMsg |> List.collect id |> Cmd.batch

    let update msg model =
        match msg with
        | SearchPageMsg msg ->
            let searchModel, cmdMsgs = SearchPage.update msg model.SearchModel
            { model with SearchModel = searchModel }, [ SubpageCmdMsgs [ SearchCmdMsgs cmdMsgs ] ]
        | FavoritesPageMsg msg ->
            let favoritesModel, cmdMsgs = FavoritesPage.update msg model.FavoritesModel

            { model with
                FavoritesModel = favoritesModel },
            [ SubpageCmdMsgs [ FavoritesCmdMsgs cmdMsgs ] ]

    let view model =
        (TabControl(Dock.Bottom) {
            TabItem("Search", View.map SearchPageMsg (SearchPage.view model.SearchModel))
            TabItem("Favorites", View.map FavoritesPageMsg (FavoritesPage.view model.FavoritesModel))
        })
            .centerHorizontal()

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
