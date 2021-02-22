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

    type Annotation = Annotation of string

    type Character =
        { name: string
          annotation: Annotation option }

    type SingleDialogue =
        { character: Character
          content: string }

    type DualDialogue =
        { characters: Character list
          content: string }

    type Dialogue =
        | Single of SingleDialogue
        | Dual of DualDialogue
        | Group of Dialogue list

    type Transition = string

    type Action =
        | Description of char seq
        | Behavior of Character array * char seq

    type Line =
        | Definition of Definition
        | SceneHeading of SceneHeading
        | Dialogue of Dialogue
        | Transition of Transition
        | Action of Action

    type Document = Line seq

    type Source = { chars: char seq; offset: int }

    type ParseError = { offset: int; message: string }

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
                    { offset = source.offset
                      message = $"expected }}, but found {char}" }
            | None ->
                Error
                    { offset = source.offset
                      message = $"expected }}, but found nothing" }
        | Some char ->
            Error
                { offset = source.offset
                  message = $"expected {{, but found {char}" }
        | None ->
            Error
                { offset = source.offset
                  message = $"expected {{, but found nothing" }

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
                    { offset = source.offset
                      message = $"expected ), but found {char}" }
            | None ->
                Error
                    { offset = source.offset
                      message = $"expected ), but found nothing" }
        | Some char ->
            Error
                { offset = source.offset
                  message = $"expected (, but found {char}" }
        | None ->
            Error
                { offset = source.offset
                  message = $"expected (, but found nothing" }

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
                        { offset = newSource.offset
                          message = $"expected =, but found {char}" }
                | None ->
                    Error
                        { offset = newSource.offset
                          message = $"expected =, but found nothing" }
            | Ok (List keys, _) ->
                Error
                    { offset = source.offset
                      message = $"expected string as key of definiation, but found list {keys}" }
            | Error error -> Error error
        | Some char ->
            Error
                { offset = source.offset
                  message = $"expected $, but found {char}" }
        | None ->
            Error
                { offset = source.offset
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
                            match str with
                            | "INT" -> Interior str
                            | "EXT" -> Exterior str
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
                    { offset = source.offset
                      message = $"expected ], but found {char}" }
            | None ->
                Error
                    { offset = source.offset
                      message = $"expected ], but found nothing" }
        | Some char ->
            Error
                { offset = source.offset
                  message = $"expected [, but found {char}" }
        | None ->
            Error
                { offset = source.offset
                  message = $"expected [, but found nothing" }
