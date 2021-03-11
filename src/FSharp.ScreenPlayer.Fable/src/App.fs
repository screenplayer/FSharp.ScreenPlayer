module App

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props

open FSharp.ScreenPlayer.Lang

open ScreenPlayer

Fable.Core.JsInterop.importAll "./App.css"

type Model =
    { editorModel: Editor.Model
      playerModel: Player.Model }

type Msg =
    | EditorMsg of Editor.Msg
    | PlayerMsg of Player.Msg

let filterLinebreak (line: Line) =
    match line with
    | Action LineBreak -> false
    | _ -> true

let init () =
    { editorModel = Editor.init "ScreenPlay" ""
      playerModel = Player.init [] },
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
                      chars = model.editorModel.content }

                match parse [||] source with
                | Ok lines ->
                    lines
                    |> Seq.filter filterLinebreak
                | _ -> Seq.empty

            let playerModel =
                { model.playerModel with
                      lines = lines
                      index = 0 }

            { model with
                  editorModel = editorModel
                  playerModel = playerModel },
            Cmd.map EditorMsg editorCmd
        | _ ->
            let (editorModel, editorCmd) =
                Editor.update editorMsg model.editorModel

            { model with editorModel = editorModel }, Cmd.map EditorMsg editorCmd
    | PlayerMsg playerMsg ->
        let (playerModel, playerCmd) =
            Player.update playerMsg model.playerModel

        { model with playerModel = playerModel }, Cmd.map PlayerMsg playerCmd

let view (model: Model) (dispatch: Dispatch<Msg>) =
    div [ Class "screenplay" ] [
        div [ Class "portal" ] [
            div [ Class "container" ] [
                Player.view model.playerModel (dispatch << PlayerMsg)
            ]
        ]
        div [ Class "container" ] [
            Editor.view model.editorModel (dispatch << EditorMsg)
        ]
    ]

Program.mkProgram init update view
|> Program.withReactBatched "app"
|> Program.run
