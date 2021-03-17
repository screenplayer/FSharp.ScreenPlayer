// Learn more about F# at http://fsharp.org

open System
open System.IO
open FSharp.ScreenPlayer.Lang

[<EntryPoint>]
let main argv =
    let currDir = Directory.GetCurrentDirectory()
    let path = Path.Combine(currDir, argv.[0])
    let text = File.ReadAllText path

    let data =
        seq {
            for char in text do
                yield char
        }


    match parse Seq.empty { line = 0; offset = 0; chars = data } with
    | Ok lines ->
        for line in lines do
            match line with
            | Definition definition ->
                printfn "Definition"
                printfn "name is %A, value is %A" definition.name definition.value
                printfn "\n"
            | SceneHeading sceneHeading ->
                printfn "scene heading: "
                printfn "%A" sceneHeading
                printfn "\n"
            | Dialogue dialogues ->
                printfn "dialogues"
                for dialogue in dialogues do
                    printfn "%A says: %A" dialogue.characters dialogue.contents
                printfn "\n"
            | Transition transition ->
                printfn "transition"
                printfn "%A" transition
                printfn "\n"
            | Action action ->
                printfn "action"
                printfn "%A" action
                printfn "\n"
    | Error error -> printfn "error: %A" error

    0 // return an integer exit code
