module App

open Elmish
open Elmish.React
open Fable.React

open FSharp.ScreenPlayer.Lang

open ScreenPlayer

type Model =
    { editorModel: Editor.Model }

type Msg =
    | EditorMsg of Editor.Msg
    | PlayerMsg of Player.Msg

let init () =
    { editorModel = { content = "" } }, Cmd.none

let update (msg: Msg) (model: Model) =
    match msg with
    | EditorMsg editorMsg ->
        match editorMsg with
        | Editor.Msg.UpdateContent content ->
            { model with editorModel = { model.editorModel with content = content } }, Cmd.none

let view (model: Model) (dispatch: Dispatch<Msg>) =
    let lines =
        let source =
            { offset = 0
              chars = model.editorModel.content }
        match parse [||] source with
        | Ok lines ->
            lines
        | _ ->
            Seq.empty

    div [] [
        Editor.view model.editorModel (dispatch << EditorMsg)
        Player.view { lines = lines } (dispatch << PlayerMsg)
    ]

Program.mkProgram init update view
|> Program.withReactBatched "app"
|> Program.run

