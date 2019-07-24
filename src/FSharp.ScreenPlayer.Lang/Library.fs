namespace FSharp.ScreenPlayer

module Lang =
    type Value =
        | Literal of string
        | Multiple of string array

    type Character = string

    type SceneHeading = string

    type Dialogue =
        | Single of Character * string
        | Multiple of Dialogue array

    type Action =
        | Description of string
        | Behavior of Character array * string

    type Line =
        | MetaData of string * Value
        | SceneHeading of SceneHeading
        | Dialogue of Dialogue
        | Transition of string
        | Action of string

    type Document = Line seq

    let split (sep: string) (str: string) =
        let strLen, sepLen = str.Length, sep.Length
        let rec loop (n: int) =
            seq {
                let index = str.IndexOf(sep, n)
                if index = -1 then
                    yield str.Substring(n)
                else
                    yield str.Substring(n, index - n)
                    yield! loop (index + sepLen)
            }

        loop 0

    let parseLine (doc: Document) (line: string) =
        doc

    let parse (doc: string) =
        let lines = split "\n" doc
        Seq.fold parseLine Seq.empty lines
