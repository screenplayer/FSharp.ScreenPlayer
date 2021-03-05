module ScreenPlayer.Player

open Elmish

open Fable.React
open Fable.React.Props
open Fable.React.Helpers
open Fable.React.Standard
open Browser.Types
open FSharp.ScreenPlayer.Lang

type Model = { lines: Line seq }

type Msg =
    | PreviousLine
    | NextLine

let lineView (line: Line) =
    match line with
    | Definition definition -> p [] [ str "definition" ]
    | SceneHeading sceneHeading -> p [] [ str "scene heading" ]
    | Dialogue dialogues -> p [] [ str "dialogues" ]
    | Transition transition -> p [] [ str "transition" ]
    | Action action -> p [] [ str "action" ]

let view (model: Model) (dispatch: Dispatch<Msg>) =
    div [ Class "screenplay__player" ] (Seq.map lineView model.lines)
