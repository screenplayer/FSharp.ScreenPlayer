module ScreenPlayer.Player

open System

open Elmish

open Fable.React
open Fable.React.Props
open Fable.React.Helpers
open Fable.React.Standard
open Browser.Types
open FSharp.ScreenPlayer.Lang

Fable.Core.JsInterop.importAll "./Player.css"


module Line =
    let definitionView (definition: Definition) =
        let text =
            match definition.value with
            | Str str -> str
            | List strs -> String.Join(" / ", strs)

        div [ Class "definition" ] [
            p [] [ str definition.name ]
            h4 [] [ str text ]
        ]

    let sceneHeadingView (sceneHeading: SceneHeading) =
        let text = sceneHeading.ToString()

        div [ Class "scene" ] [
            h1 [] [ str text ]
        ]

    let characterView (character: Character) =
        let annotation =
            match character.annotation with
            | Some (Annotation text) -> text
            | None -> ""

        div [ Class "character" ] [
            div [ Class "character__header" ] [
                span [ Class "character__name" ] [
                    str character.name
                ]
                span [ Class "character__annotation" ] [
                    str annotation
                ]
            ]
        ]

    let contentView (content: DialogueContent) =
        match content with
        | DialogueText text ->
            p [ Class "dialogue__text" ] [
                str text
            ]
        | DialogueAnnotation (Annotation annotation) ->
            p [ Class "dialogue__annotation" ] [
                str annotation
            ]

    let dialogueView (dialogue: Dialogue) =
        let characters =
            Array.map characterView dialogue.characters

        let contents = Array.map contentView dialogue.contents

        div [ Class "dialogue" ] [
            div [ Class "characters" ] characters
            div [ Class "contents" ] contents
        ]

    let transitionView (transition: Transition) = p [] [ str transition.name ]

    let actionView (action: Action) =
        let text =
            match action with
            | LineBreak -> ""
            | Description description -> description
            | Behavior (characters, description) -> String.Join(", ", characters) + " " + description

        p [ Class "action" ] [ str text ]

    let view (line: Line) =
        match line with
        | Definition definition -> definitionView definition
        | SceneHeading sceneHeading -> sceneHeadingView sceneHeading
        | Dialogue dialogues -> div [ Class "dialogues" ] (Array.map dialogueView dialogues)
        | Transition transition -> transitionView transition
        | Action action -> actionView action


type Model = { lines: Line seq; index: int32 }

type Msg =
    | PreviousLine
    | NextLine

let init (lines: Line seq) = { lines = lines; index = 0 }

let update (msg: Msg) (model: Model) =
    match msg with
    | PreviousLine -> { model with index = model.index - 1 }, Cmd.none
    | NextLine -> { model with index = model.index + 1 }, Cmd.none

let view (model: Model) (dispatch: Dispatch<Msg>) =

    let handlePrevious (evt: Event) = dispatch PreviousLine

    let handleNext (evt: Event) = dispatch NextLine

    let lineView =
        match Seq.tryItem model.index model.lines with
        | Some line -> Line.view line
        | _ -> div [] []

    div [ Class "screenplay__player" ] [
        div [ Class "screenplay__player__screen" ] [
            div [ Class "screenplay__player__content" ] [
                lineView
            ]
        ]
        footer [ Class "screenplay__player__actions" ] [
            button [ Disabled(model.index = 0)
                     OnClick handlePrevious
                     Class "button button--primary" ] [
                str "Prev"
            ]
            button [ Disabled(model.index >= (Seq.length model.lines - 1))
                     OnClick handleNext
                     Class "button button--primary" ] [
                str "Next"
            ]
        ]
    ]
