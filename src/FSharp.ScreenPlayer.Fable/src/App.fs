module App

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props

open FSharp.ScreenPlayer.Lang

open ScreenPlayer

Fable.Core.JsInterop.importAll "./App.css"

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

let filterLinebreak (line: Line) =
    match line with
    | Action LineBreak -> false
    | _ -> true

let init () =
    { editorModel = Editor.init "ScreenPlay" ""
      playerModel = Player.init []
      mode = Edit },
    Cmd.none

let update (msg: Msg) (model: Model) =
    match msg with
    | EditorMsg editorMsg ->
        match editorMsg with
        | Editor.Msg.Update content ->
            let (editorModel, editorCmd) =
                Editor.update editorMsg model.editorModel

            let lines =
                let source =
                    { offset = 0
                      chars = editorModel.content }

                match parse [||] source with
                | Ok lines -> lines |> Seq.filter filterLinebreak
                | _ -> Seq.empty

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
        | _ ->
            let (editorModel, editorCmd) =
                Editor.update editorMsg model.editorModel

            { model with editorModel = editorModel }, Cmd.map EditorMsg editorCmd
    | PlayerMsg playerMsg ->
        let (playerModel, playerCmd) =
            Player.update playerMsg model.playerModel

        { model with playerModel = playerModel }, Cmd.map PlayerMsg playerCmd
    | Dismiss ->
        { model with mode = Edit }, Cmd.none

let view (model: Model) (dispatch: Dispatch<Msg>) =
    let handleDismiss evt = dispatch Dismiss

    let portalState =
        match model.mode with
        | Edit -> "hidden"
        | Preview -> "shown"

    div [ Class "screenplay" ] [
        div [ Class $"portal portal--{portalState}" ] [
            div [ Class "portal__backdrop"; OnClick handleDismiss ] []
            div [ Class "portal__body" ] [
                div [ Class "container" ] [
                    Player.view model.playerModel (dispatch << PlayerMsg)
                ]
            ]
        ]
        div [ Class "container" ] [
            Editor.view model.editorModel (dispatch << EditorMsg)
        ]
    ]

Program.mkProgram init update view
|> Program.withReactBatched "app"
|> Program.run
