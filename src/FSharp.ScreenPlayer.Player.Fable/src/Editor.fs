module ScreenPlayer.Editor

open Elmish

open Fable.React
open Fable.React.Props
open Fable.React.Helpers
open Fable.React.Standard
open Browser.Types

Fable.Core.JsInterop.importAll "./Editor.css"

type Model = { title: string; content: string }

type Msg =
    | Preview
    | Update of string
    | Cancel
    | Submit

let init (title: string) (content: string) =
    { title = title; content = content }

let update (msg: Msg) (model: Model) =
    match msg with
    | Update content ->
        { model with content = content }, Cmd.none
    | _ ->
        model, Cmd.none

let view (model: Model) (dispatch: Dispatch<Msg>) =
    let handleInput (evt: Event) =
        dispatch (Update (evt.target :?> HTMLTextAreaElement).value)

    let handleCancel (evt: Event) = dispatch Cancel

    let handleSubmit (evt: Event) = dispatch Submit

    div [ Class "screenplay__editor" ] [
        header [ Class "screenplay__editor__header" ] [
            h4 [] [ str model.title ]
            button [ Class "button button--primary" ] [
                str "Preview"
            ]
        ]
        textarea [ Class "screenplay__editor__input"
                   OnInput handleInput
                   Value model.content ] []
        footer [ Class "screenplay__editor__actions" ] [
            button [ Class "button button--outline"; OnClick handleCancel ] [
                str "Cancel"
            ]
            button [ Class "button button--primary"; OnClick handleSubmit ] [
                str "Save"
            ]
        ]
    ]
