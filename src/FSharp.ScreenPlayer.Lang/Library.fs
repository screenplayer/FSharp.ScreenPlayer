namespace FSharp.ScreenPlayer

open System

module Chars =
    let toString (chars: char seq) = chars |> Seq.toArray |> String

module Lang =
    type Value =
        | Str of string
        | List of string array

    type Definition = { name: string; value: Value }

    type SceneHeading =
        | Interior of string
        | Exterior of string
        | Other of string
        | Nested of SceneHeading array

        override self.ToString() =
            match self with
            | Interior "" -> "Interior"
            | Interior interior -> $"Interior - {interior}"
            | Exterior "" -> "Exterior"
            | Exterior exterior -> $"Exterior - {exterior}"
            | Other sceneheading -> $"{sceneheading}"
            | Nested scenes -> String.Join(" / ", Array.map (fun scene -> scene.ToString()) scenes)

    type Annotation = Annotation of string

    type Character =
        { name: string
          annotation: Annotation option }

        static member create (name: string) (annotation: Annotation option) =
            { name = name; annotation = annotation }

    type DialogueContent =
        | DialogueAnnotation of Annotation
        | DialogueText of string

    type Dialogue =
        { characters: Character array
          contents: DialogueContent array }


    type Transition = { name: string }

    type Action =
        | LineBreak
        | Description of string
        | Behavior of Character array * string

    type Line =
        | Definition of Definition
        | SceneHeading of SceneHeading
        | Dialogue of Dialogue array
        | Transition of Transition
        | Action of Action

    type Source =
        { chars: char seq
          line: int
          offset: int }

        static member skipWhitespaces(source: Source) =
            let whitespaces =
                Seq.takeWhile (fun char -> char = ' ') source.chars

            let count = Seq.length whitespaces

            { source with
                  chars = Seq.skip count source.chars
                  offset = source.offset + count }

    type ParseError = { line: int; offset: int; message: string }

    let parseValue (source: Source) =
        match Seq.tryHead source.chars with
        | Some '{' ->
            let start = Seq.tail source.chars

            let chars =
                Seq.takeWhile (fun char -> char <> '}') start

            let valueLen = Seq.length chars
            let tail = Seq.skip valueLen start

            match Seq.tryHead tail with
            | Some '}' ->
                let value = Chars.toString chars

                let parts =
                    value.Split ';'
                    |> Array.map (fun str -> str.Trim())

                if Array.length parts = 1 then
                    Ok(
                        Str parts.[0],
                        { source with
                              chars = Seq.skip (valueLen + 2) source.chars
                              offset = source.offset + valueLen + 2 }
                    )
                else
                    Ok(
                        List parts,
                        { source with
                              chars = Seq.skip (valueLen + 2) source.chars
                              offset = source.offset + valueLen + 2 }
                    )
            | Some char ->
                Error
                    { line = source.line
                      offset = source.offset
                      message = $"expected }}, but found {char}" }
            | None ->
                Error
                    { line = source.line
                      offset = source.offset
                      message = $"expected }}, but found nothing" }
        | Some char ->
            Error
                { line = source.line
                  offset = source.offset
                  message = $"expected {{, but found {char}" }
        | None ->
            Error
                { line = source.line
                  offset = source.offset
                  message = $"expected {{, but found nothing" }

    let parseDefinition (source: Source) =
        match Seq.tryHead source.chars with
        | Some '$' ->
            match parseValue
                      { source with
                            chars = Seq.tail source.chars
                            offset = source.offset + 1 } with
            | Ok (Str key, newSource) ->
                match Seq.tryHead newSource.chars with
                | Some '=' ->
                    match parseValue
                              { newSource with
                                    chars = Seq.tail newSource.chars
                                    offset = newSource.offset + 1 } with
                    | Ok (value, newSource) -> Ok({ name = key; value = value }, newSource)
                    | Error error -> Error error
                | Some char ->
                    Error
                        { line = newSource.line
                          offset = newSource.offset
                          message = $"expected =, but found {char}" }
                | None ->
                    Error
                        { line = newSource.line
                          offset = newSource.offset
                          message = $"expected =, but found nothing" }
            | Ok (List keys, _) ->
                Error
                    { line = source.line
                      offset = source.offset
                      message = $"expected string as key of definiation, but found list {keys}" }
            | Error error -> Error error
        | Some char ->
            Error
                { line = source.line
                  offset = source.offset
                  message = $"expected $, but found {char}" }
        | None ->
            Error
                { line = source.line
                  offset = source.offset
                  message = $"expected $, but found nothing" }

    let parseSceneHeading (source: Source) =
        match Seq.tryHead source.chars with
        | Some '[' ->
            let start = Seq.tail source.chars

            let chars =
                Seq.takeWhile (fun char -> char <> ']') start

            let valueLen = Seq.length chars
            let tail = Seq.skip valueLen start

            match Seq.tryHead tail with
            | Some ']' ->
                let value = Chars.toString chars

                let parts =
                    value.Split '/'
                    |> Array.map
                        (fun str ->
                            let parts = str.Split '-'

                            match parts.[0].Trim() with
                            | "INT" ->
                                match Array.tryItem 1 parts with
                                | Some details -> Interior(details.Trim())
                                | _ -> Interior ""
                            | "EXT" ->
                                match Array.tryItem 1 parts with
                                | Some details -> Exterior(details.Trim())
                                | _ -> Exterior ""
                            | _ -> Other str)

                if Array.length parts = 1 then
                    Ok(
                        parts.[0],
                        { source with
                              chars = Seq.skip (valueLen + 2) source.chars
                              offset = source.offset + valueLen + 2 }
                    )
                else
                    Ok(
                        Nested parts,
                        { source with
                              chars = Seq.skip (valueLen + 2) source.chars
                              offset = source.offset + valueLen + 2 }
                    )
            | Some char ->
                Error
                    { line = source.line
                      offset = source.offset
                      message = $"expected ], but found {char}" }
            | None ->
                Error
                    { line = source.line
                      offset = source.offset
                      message = $"expected ], but found nothing" }
        | Some char ->
            Error
                { line = source.line
                  offset = source.offset
                  message = $"expected [, but found {char}" }
        | None ->
            Error
                { line = source.line
                  offset = source.offset
                  message = $"expected [, but found nothing" }

    let parseAnnotation (source: Source) =
        match Seq.tryHead source.chars with
        | Some '(' ->
            let start = Seq.tail source.chars

            let chars =
                Seq.takeWhile (fun char -> char <> ')') start

            let valueLen = Seq.length chars
            let tail = Seq.skip valueLen start

            match Seq.tryHead tail with
            | Some ')' ->
                let value = Chars.toString chars

                Ok(
                    Annotation value,
                    { source with
                          chars = Seq.skip (valueLen + 2) source.chars
                          offset = source.offset + valueLen + 2 }
                )
            | Some char ->
                Error
                    { line = source.line
                      offset = source.offset
                      message = $"expected ), but found {char}" }
            | None ->
                Error
                    { line = source.line
                      offset = source.offset
                      message = $"expected ), but found nothing" }
        | Some char ->
            Error
                { line = source.line
                  offset = source.offset
                  message = $"expected (, but found {char}" }
        | None ->
            Error
                { line = source.line
                  offset = source.offset
                  message = $"expected (, but found nothing" }

    let parseCharacters (source: Source) =
        match Seq.tryHead source.chars with
        | Some '@' ->
            match parseValue
                      { source with
                            chars = Seq.tail source.chars
                            offset = source.offset + 1 } with
            | Ok (value, newSource) ->
                match Seq.tryHead newSource.chars with
                | Some '(' ->
                    match parseAnnotation newSource with
                    | Ok (annotation, newSource2) ->
                        match value with
                        | Str name ->
                            let character =
                                { name = name
                                  annotation = Some annotation }

                            Ok([| character |], newSource2)
                        | List names ->
                            let characters =
                                Array.map (fun name -> Character.create name (Some annotation)) names

                            Ok(characters, newSource2)
                    | _ ->
                        match value with
                        | Str name ->
                            let character = { name = name; annotation = None }
                            Ok([| character |], newSource)
                        | List names ->
                            let characters =
                                Array.map (fun name -> Character.create name None) names

                            Ok(characters, newSource)
                | _ ->
                    match value with
                    | Str name ->
                        let character = { name = name; annotation = None }
                        Ok([| character |], newSource)
                    | List names ->
                        let characters =
                            Array.map (fun name -> Character.create name None) names

                        Ok(characters, newSource)
            | Error error -> Error error
        | Some char ->
            Error
                { line = source.line
                  offset = source.offset
                  message = $"expected @, but found {char}" }
        | None ->
            Error
                { line = source.line
                  offset = source.offset
                  message = $"expected @, but found nothing" }

    let rec parseDialogueContent (contents: DialogueContent array) (source: Source) =
        match Seq.tryHead source.chars with
        | Some '"' ->
            let start = Seq.tail source.chars

            let chars =
                Seq.takeWhile (fun char -> char <> '"') start

            let valueLen = Seq.length chars
            let tail = Seq.skip valueLen start

            match Seq.tryHead tail with
            | Some '"' ->
                let text = Chars.toString chars

                let newSource =
                    { source with
                          chars = Seq.skip (valueLen + 2) source.chars
                          offset = source.offset + valueLen + 2 }
                    |> Source.skipWhitespaces

                match Seq.tryHead newSource.chars with
                | Some '(' ->
                    match parseAnnotation newSource with
                    | Ok (annotation, newSource2) ->
                        let newContents =
                            Array.append
                                contents
                                [| DialogueText text
                                   DialogueAnnotation annotation |]

                        let newSource3 = Source.skipWhitespaces newSource2

                        match Seq.tryHead newSource3.chars with
                        | Some '"' -> parseDialogueContent newContents newSource3
                        | _ -> Ok(newContents, newSource2)
                    | Error error -> Error error
                | _ ->
                    Ok(
                        Array.append contents [| DialogueText text |],
                        { source with
                              chars = Seq.skip (valueLen + 2) source.chars
                              offset = source.offset + valueLen + 2 }
                    )
            | Some char ->
                Error
                    { line = source.line
                      offset = source.offset
                      message = $"expected \", but found {char}" }
            | None ->
                Error
                    { line = source.line
                      offset = source.offset
                      message = $"expected \", but found nothing" }
        | Some char ->
            Error
                { line = source.line
                  offset = source.offset
                  message = $"expected \", but found {char}" }
        | None ->
            Error
                { line = source.line
                  offset = source.offset
                  message = $"expected \", but found nothing" }

    let rec parseDialogue (dialogues: Dialogue array) (source: Source) =
        match Seq.tryHead source.chars with
        | Some '@' ->
            match parseCharacters source with
            | Ok (characters, newSource) ->
                let newSource2 = Source.skipWhitespaces newSource

                match Seq.tryHead newSource2.chars with
                | Some '"' ->
                    match parseDialogueContent [||] newSource2 with
                    | Ok (dialogueContent, newSource3) ->
                        let dialogue =
                            { characters = characters
                              contents = dialogueContent }

                        let newSource4 = Source.skipWhitespaces newSource3

                        match Seq.tryHead newSource4.chars with
                        | Some '&' ->
                            let newSource5 =
                                { newSource4 with
                                      offset = newSource4.offset + 1
                                      chars = Seq.tail newSource4.chars }
                                |> Source.skipWhitespaces

                            parseDialogue (Array.append dialogues [| dialogue |]) newSource5
                        | _ -> Ok(Array.append dialogues [| dialogue |], newSource4)
                    | Error error -> Error error
                | Some char ->
                    Error
                        { line = source.line
                          offset = source.offset
                          message = $"expected @, but found {char}" }
                | None ->
                    Error
                        { line = source.line
                          offset = source.offset
                          message = $"expected @, but found nothing" }
            | Error error -> Error error
        | Some char ->
            Error
                { line = source.line
                  offset = source.offset
                  message = $"expected @, but found {char}" }
        | None ->
            Error
                { line = source.line
                  offset = source.offset
                  message = $"expected @, but found nothing" }

    let parseTransition (source: Source) =
        match Seq.tryHead source.chars with
        | Some '>' ->
            let start = Seq.tail source.chars

            let chars =
                Seq.skipWhile (fun char -> char = ' ') start
                |> Seq.takeWhile (fun char -> char <> '\n')

            let value = Chars.toString chars
            let valueLen = Seq.length chars

            Ok(
                { name = value },
                { source with
                      chars = Seq.skip (valueLen + 2) source.chars
                      offset = source.offset + valueLen + 2 }
            )
        | Some char ->
            Error
                { line = source.line
                  offset = source.offset
                  message = $"expected >, but found {char}" }
        | None ->
            Error
                { line = source.line
                  offset = source.offset
                  message = $"expected >, but found v" }


    let parseAction (source: Source) =
        let chars =
            Seq.takeWhile (fun char -> char <> '\n' && char <> '\r') source.chars

        let value = Chars.toString chars

        let valueLen = Seq.length chars

        Ok(
            Description value,
            { source with
                  chars = Seq.skip valueLen source.chars
                  offset = source.offset + valueLen }
        )

    let rec parse (lines: Line seq) (source: Source) =
        if Seq.isEmpty source.chars then
            Ok lines
        else
            match Seq.tryHead source.chars with
            | Some '$' ->
                match parseDefinition source with
                | Ok (definition, newSource) ->
                    let line = Definition definition
                    parse (Seq.append lines [ line ]) newSource
                | Error error -> Error error
            | Some '[' ->
                match parseSceneHeading source with
                | Ok (sceneHeading, newSource) ->
                    let line = SceneHeading sceneHeading
                    parse (Seq.append lines [ line ]) newSource
                | Error error -> Error error
            | Some '>' ->
                match parseTransition source with
                | Ok (transition, newSource) ->
                    let line = Transition transition
                    parse (Seq.append lines [ line ]) newSource
                | Error error -> Error error
            | Some '@' ->
                match parseCharacters source with
                | Ok (characters, newSource) ->
                    let newSource2 = Source.skipWhitespaces newSource

                    match Seq.tryHead newSource2.chars with
                    | Some '"' ->
                        match parseDialogue [||] source with
                        | Ok (dialogues, newSource3) -> parse (Seq.append lines [ Dialogue dialogues ]) newSource3
                        | Error error -> Error error
                    | Some _ ->
                        match parseAction source with
                        | Ok (action, newSource) -> parse (Seq.append lines [| Action action |]) newSource
                        | Error error -> Error error
                    | None ->
                        Error
                            { line = newSource2.line
                              offset = newSource2.offset
                              message = "expected dialogue or actions" }
                | Error error -> Error error
            | Some '\r' ->
                match Seq.tryHead (Seq.tail source.chars) with
                | Some '\n' ->
                    parse
                        (Seq.append lines [| Action LineBreak |])
                        { source with
                            line = source.line + 1
                            chars = Seq.skip 2 source.chars
                            offset = source.offset + 2 }
                | _ ->
                    match parseAction source with
                    | Ok (action, newSource) -> parse (Seq.append lines [| Action action |]) newSource
                    | Error error -> Error error
            | Some '\n' ->
                parse
                    (Seq.append lines [| Action LineBreak |])
                    { source with
                        line = source.line + 1
                        chars = Seq.tail source.chars
                        offset = source.offset + 1 }
            | _ ->
                match parseAction source with
                | Ok (action, newSource) -> parse (Seq.append lines [| Action action |]) newSource
                | Error error -> Error error
