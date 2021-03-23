module App

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props

open FSharp.ScreenPlayer.Lang

open ScreenPlayer
open Fable.Core.JS
open Browser.WebStorage

Fable.Core.JsInterop.importAll "./App.css"

let draftStorageKey = "screenplayer.draft"

type Mode =
    | Preview
    | Edit

type Model =
    { editorModel: Editor.Model
      playerModel: Player.Model
      mode: Mode }

type Msg =
    | EditorMsg of Editor.Msg
    | PlayerMsg of Player.Msg
    | Dismiss
    | AutoSave

let filterLinebreak (line: Line) =
    match line with
    | Action LineBreak -> false
    | _ -> true

let init () =
    let content = localStorage.getItem draftStorageKey

    { editorModel = Editor.init "ScreenPlayer" content
      playerModel = Player.init []
      mode = Edit },
    Cmd.none

let update (msg: Msg) (model: Model) =
    match msg with
    | EditorMsg editorMsg ->
        match editorMsg with
        | Editor.Msg.Update (content, height) ->
            let (editorModel, editorCmd) =
                Editor.update editorMsg model.editorModel

            let lines =
                let source =
                    { line = 0
                      offset = 0
                      chars = editorModel.content }

                match parse [||] source with
                | Ok lines -> lines |> Seq.filter filterLinebreak
                | _ -> model.playerModel.lines

            let playerModel =
                { model.playerModel with
                      lines = lines
                      index = 0 }

            { model with
                  editorModel = editorModel
                  playerModel = playerModel },
            Cmd.map EditorMsg editorCmd
        | Editor.Msg.Preview ->
            let (editorModel, editorCmd) =
                Editor.update editorMsg model.editorModel

            { model with
                  editorModel = editorModel
                  mode = Preview },
            Cmd.map EditorMsg editorCmd
        | Editor.Msg.Save ->
            localStorage.setItem (draftStorageKey, model.editorModel.content)
            model, Cmd.none
    | PlayerMsg playerMsg ->
        let (playerModel, playerCmd) =
            Player.update playerMsg model.playerModel

        { model with playerModel = playerModel }, Cmd.map PlayerMsg playerCmd
    | Dismiss -> { model with mode = Edit }, Cmd.none
    | AutoSave ->
        localStorage.setItem (draftStorageKey, model.editorModel.content)
        model, Cmd.none

let view (model: Model) (dispatch: Dispatch<Msg>) =
    let handleDismiss evt = dispatch Dismiss

    let portalState =
        match model.mode with
        | Edit -> "hidden"
        | Preview -> "shown"

    div [ Class "page" ] [
        Header.view
        div [ Class "page__body" ] [
            div [ Class "screenplay" ] [
                div [ Class "container" ] [
                    Editor.view model.editorModel (dispatch << EditorMsg)
                ]
            ]
        ]
        div [ Class $"portal portal--{portalState}" ] [
            div [ Class "portal__backdrop"
                  OnClick handleDismiss ] []
            div [ Class "portal__body" ] [
                div [ Class "container" ] [
                    Player.view model.playerModel (dispatch << PlayerMsg)
                ]
            ]
        ]
    ]

let timer interval initial =
    let sub dispatch =
        setInterval (fun _ -> dispatch AutoSave) interval
        |> ignore

    Cmd.ofSub sub

Program.mkProgram init update view
|> Program.withReactBatched "app"
|> Program.withSubscription (timer 20000)
|> Program.run
