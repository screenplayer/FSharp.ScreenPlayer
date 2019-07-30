// Learn more about F# at http://fsharp.org

open System
open System.IO
open FSharp.ScreenPlayer.Lang

[<EntryPoint>]
let main argv =
    let currDir = Directory.GetCurrentDirectory()
    let path = Path.Combine (currDir, "./data/test.scp")
    let lines =
       File.ReadAllLines path
        |> Array.toSeq
    let data =
        seq {
            for line in lines do
                for char in line do
                    yield char
        }
    let doc = parse Seq.empty data
    for line in doc do
        match line with
        | Definition (key, value) ->
            printfn "Definition key is %A, value is %A" key value
        | SceneHeading sceneHeading ->
            printfn "scene heading: %A" sceneHeading
        | Dialogue (character, words) ->
            printfn "dialogue %A says: %A" character words
        | Transition _transition ->
            printfn "transition"
        | Action action ->
            printfn "action: %A" action
    0 // return an integer exit code
