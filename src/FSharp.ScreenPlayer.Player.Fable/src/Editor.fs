module ScreenPlayer.Editor

open Elmish

open Fable.React
open Fable.React.Props
open Browser.Types

Fable.Core.JsInterop.importAll "./Editor.css"

type Model = { title: string; content: string; textareaHeight: float option }

type Msg =
    | Update of string * float
    | Preview
    | Save

let init (title: string) (content: string) =
    { title = title; content = content; textareaHeight = None }

let update (msg: Msg) (model: Model) =
    match msg with
    | Update (content, height) ->
        { model with content = content; textareaHeight = Some height }, Cmd.none
    | _ ->
        model, Cmd.none

let view (model: Model) (dispatch: Dispatch<Msg>) =
    let handleInput (evt: Event) =
        let content = (evt.target :?> HTMLTextAreaElement).value
        let height = (evt.target :?> HTMLTextAreaElement).scrollHeight
        dispatch (Update (content, height))

    let handleSave (evt: Event) = dispatch Save

    let handlePreview (evt: Event) = dispatch Preview

    let textareaStyle =
        match model.textareaHeight with
        | Some height ->
            [ Height height ]
        | None ->
            []

    div [ Class "screenplay__editor" ] [
        textarea [ Class "screenplay__editor__input"
                   OnInput handleInput
                   DefaultValue model.content
                   Style textareaStyle ] []
        footer [ Class "screenplay__editor__actions" ] [
            button [ Type "button"; Class "button button--primary"; OnClick handleSave ] [
                str "Save"
            ]
            button [ Type "button"; Class "button button--primary"; OnClick handlePreview ] [
                str "Preview"
            ]
        ]
    ]
