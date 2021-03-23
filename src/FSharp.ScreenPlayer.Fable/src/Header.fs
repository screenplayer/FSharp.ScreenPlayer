module Header

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props

Fable.Core.JsInterop.importAll "./Header.css"

let view =
    header [ Class "page__header" ] [
        div [ Class "container" ] [
            h4 [] [
                span [] [ str "ScreenPlayer" ]
                a [ Href "https://github.com/screenplayer/FSharp.ScreenPlayer/blob/master/docs/standard/index.md" ] [ str "Syntax" ]
            ]
        ]
    ]
