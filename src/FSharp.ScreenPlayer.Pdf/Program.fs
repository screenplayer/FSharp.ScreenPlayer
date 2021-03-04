// Learn more about F# at http://fsharp.org

open System
open System.IO
open MigraDoc.Rendering
open FSharp.ScreenPlayer.Lang
open MigraDoc.DocumentObjectModel

[<EntryPoint>]
let main argv =
    Text.Encoding.RegisterProvider(Text.CodePagesEncodingProvider.Instance);

    let currDir = Directory.GetCurrentDirectory()
    let path = Path.Combine(currDir, "./data/test.scp")
    let text = File.ReadAllText path

    let data =
        seq {
            for char in text do
                yield char
        }

    match parse Seq.empty { offset = 0; chars = data } with
    | Ok lines ->
        let document = Document()
        document.Styles.[StyleNames.Normal].Font.Name <- "Lucida Sans"
        let renderer = PdfDocumentRenderer(true)
        renderer.Document <- document

        for line in lines do
            match line with
            | Definition definition ->
                if document.LastSection = null then
                    document.AddSection() |> ignore
                let paragraph = document.LastSection.AddParagraph()
                let text =
                    match definition.value with
                    | Str str -> str
                    | List strs -> String.Join("; ", strs)
                paragraph.AddText(definition.name + ": " + text) |> ignore
                paragraph.AddLineBreak() |> ignore
            | SceneHeading sceneHeading ->
                let section = document.AddSection()
                let paragraph = section.AddParagraph()
                let text =
                    match sceneHeading with
                    | Interior interior -> String.Join(" - ", [|"Interior", interior|])
                    | Exterior exterior -> String.Join(" - ", [|"Exterior", exterior|])
                    | Other other -> other
                    | Nested headings -> String.Join(" / ", headings)
                paragraph.AddFormattedText(text, TextFormat.Bold) |> ignore
                paragraph.AddLineBreak() |> ignore
                paragraph.AddLineBreak() |> ignore
            | Dialogue dialogues ->
                for dialogue in dialogues do
                    let paragraph = document.LastSection.AddParagraph()
                    let names = 
                        dialogue.characters
                        |> Array.map (fun (character: FSharp.ScreenPlayer.Lang.Character) -> character.name) 
                    let text = String.Join(", ", names)
                    paragraph.Format.Alignment <- ParagraphAlignment.Center
                    paragraph.AddFormattedText(text, TextFormat.Bold) |> ignore
                    paragraph.AddLineBreak()
                    for content in dialogue.contents do
                        match content with
                        | DialogueText text ->
                            paragraph.AddFormattedText(text) |> ignore
                            paragraph.AddLineBreak()
                        | DialogueAnnotation (Annotation annotation) ->
                            paragraph.AddFormattedText("(", TextFormat.Italic) |> ignore
                            paragraph.AddFormattedText(annotation, TextFormat.Italic) |> ignore
                            paragraph.AddFormattedText(")", TextFormat.Italic) |> ignore
                            paragraph.AddLineBreak()
            | Transition transition ->
                let paragraph = document.LastSection.AddParagraph()
                paragraph.AddFormattedText(transition.name, TextFormat.Bold) |> ignore
                paragraph.AddLineBreak() |> ignore
            | Action action ->
                match action with
                | LineBreak ->
                    if document.LastSection <> null then
                        let paragraph = document.LastSection.AddParagraph()
                        paragraph.AddLineBreak() |> ignore
                | Description text ->
                    let paragraph = document.LastSection.AddParagraph()
                    paragraph.AddText(text) |> ignore
                    paragraph.AddLineBreak() |> ignore
                | Behavior (characters, text) ->
                    let paragraph = document.LastSection.AddParagraph()
                    paragraph.AddText(text) |> ignore
                    paragraph.AddLineBreak() |> ignore

        renderer.RenderDocument()

        let filename = "./data/test.pdf";
        renderer.PdfDocument.Save(filename);
    | Error error -> printfn "error: %A" error

    0 // return an integer exit code
