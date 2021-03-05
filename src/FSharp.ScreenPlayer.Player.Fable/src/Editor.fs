module ScreenPlayer.Editor

open Elmish

open Fable.React
open Fable.React.Props
open Fable.React.Helpers
open Fable.React.Standard
open Browser.Types

type Model =
    { content: string }

type Msg = UpdateContent of string

let view (model: Model) (dispatch: Dispatch<Msg>) =
    let handleInput (evt: Event) =
        dispatch (UpdateContent (evt.target :?> HTMLTextAreaElement).value)

    div [ Class "screenplay__editor" ] [
        textarea [ OnInput handleInput; Value model.content ] []
    ]
