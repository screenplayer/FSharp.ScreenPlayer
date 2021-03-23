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

let placeholder = """${Title}={PULP FICTION}
${Authors}={Quentin Tarantino; Roger Avary}

[INT. COFFEE SHOP – MORNING]

A normal Denny's, Spires-like coffee shop in Los Angeles. It's about 9:00 in the morning. While the place isn't jammed, there's a healthy number of people drinking coffee, munching on bacon and eating eggs.

Two of these people are a YOUNG MAN and a YOUNG WOMAN. The Young Man has a slight working-class English accent and, like his fellow countryman, smokes cigarettes like they're going out of style.

It is impossible to tell where the Young Woman is from or how old she is; everything she does contradicts something she did. The boy and girl sit in a booth. Their dialogue is to be said in a rapid pace "HIS GIRL FRIDAY" fashion.

@{YOUNG MAN} "No, forget it, it's too risky. I'm through doin' that shit."

@{YOUNG WOMAN} "You always say that, the same thing every time: never again, I'm through, too dangerous."

@{YOUNG MAN} "I know that's what I always say. I'm always right too, but –"

@{YOUNG WOMAN} "– but you forget about it in a day or two -"

@{YOUNG MAN} "– yeah, well, the days of me forgittin' are over, and the days of me rememberin' have just begun."

@{YOUNG WOMAN} "When you go on like this, you know what you sound like?"

@{YOUNG MAN} "I sound like a sensible fucking man, is what I sound like."

@{YOUNG WOMAN} "You sound like a duck."(imitates a duck)"Quack, quack, quack, quack, quack, quack, quack..."
"""

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
    let cache = localStorage.getItem draftStorageKey
    let content =
        if isNull cache then
            placeholder
        else
            cache

    let lines =
        let source =
            { line = 0
              offset = 0
              chars = content }

        match parse [||] source with
        | Ok lines -> lines |> Seq.filter filterLinebreak
        | Error error ->
            printfn "%A" error
            Seq.empty

    { editorModel = Editor.init "ScreenPlayer" content
      playerModel = Player.init lines
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
                | Error error ->
                    printfn "%A" error
                    model.playerModel.lines

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
