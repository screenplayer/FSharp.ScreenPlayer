namespace FSharp.ScreenPlayer

module Lang =
    type Value = char seq

    type Parenthetical = char seq

    type Character =
        { name: char seq
          parenthetical: Parenthetical option }

    type SceneHeading = char seq

    type Dialogue = Character * char seq

    type Action =
        | Description of char seq
        | Behavior of Character array * char seq

    type Line =
        | Definition of Value * Value
        | SceneHeading of SceneHeading
        | Dialogue of Dialogue
        | Transition of string
        | Action of Action

    type Document = Line seq

    let parseValue (doc: char seq) =
        let firstChar = Seq.head doc
        if firstChar = '{' then
            match Seq.tryFindIndex (fun char -> char = '}') doc with
            | Some index ->
                let value = Seq.take (index + 1) doc
                Some value
            | None ->
                None
        else
            None

    let parseHeading (doc: char seq) =
        let firstChar = Seq.head doc
        if firstChar = '[' then
            match Seq.tryFindIndex (fun char -> char = ']') doc with
            | Some index ->
                let value = Seq.take (index + 1) doc
                Some value
            | None ->
                None
        else
            None

    let parseParenthetical (doc: char seq) =
        let firstChar = Seq.head doc
        if firstChar = '(' then
            match Seq.tryFindIndex (fun char -> char = ')') (Seq.skip 1 doc) with
            | Some index ->
                let value = Seq.take (index + 2) doc
                Some value
            | None ->
                None
        else
            None

    let parseDialogue (doc: char seq) =
        let firstChar = Seq.head doc
        if firstChar = '"' then
            match Seq.tryFindIndex (fun char -> char = '"') (Seq.skip 1 doc) with
            | Some index ->
                let value = Seq.take (index + 2) doc
                Some value
            | None ->
                None
        else
            None

    let parseAction (doc: char seq) =
        match Seq.tryFindIndex (fun char -> char = '\n') doc with
        | Some index ->
            let value = Seq.take (index + 1) doc
            Some value
        | None ->
            Some doc

    type ParseResult = Result<Document, char seq>

    let rec parse (doc: Line seq) (source: char seq) =
        if Seq.isEmpty source then
            doc
        else
            let firstChar = Seq.head source
            match firstChar with
            | '$' ->
                match parseValue (Seq.skip 1 source) with
                | Some key ->
                    let left = Seq.skip ((Seq.length key) + 1) source
                    if Seq.head left = '=' then
                        match parseValue (Seq.skip 1 left) with
                        | Some value ->
                            let definition = Definition (key, value)
                            parse (Seq.append doc [definition]) (Seq.skip ((Seq.length value) + 1) left)
                        | None ->
                            doc
                    else
                        doc
                | None ->
                    doc
            | '[' ->
                match parseHeading source with
                | Some heading ->
                    let sceneHeading = SceneHeading heading
                    parse (Seq.append doc [sceneHeading]) (Seq.skip (Seq.length heading) source)
                | None ->
                    doc
            | '@' ->
                match parseValue (Seq.skip 1 source) with
                | Some name ->
                    let left = Seq.skipWhile (fun c -> c = ' ') (Seq.skip ((Seq.length name) + 1) source)
                    let nextChar = Seq.head left
                    let parenthetical =
                        if nextChar = '(' then
                            parseParenthetical left
                        else
                            None
                    let parentheticalLength =
                        match parenthetical with
                        | Some content ->
                            Seq.length content
                        | None -> 0
                    let character = { name = name; parenthetical = parenthetical }
                    let content = Seq.skipWhile (fun c -> c = ' ') (Seq.skip parentheticalLength left)
                    match Seq.head content with
                    | '"' ->
                        match parseDialogue content with
                        | Some dialogue ->
                            parse (Seq.append doc [Dialogue (character, dialogue)]) (Seq.skip (Seq.length dialogue) content)
                        | None ->
                            doc
                    | _ ->
                        match parseAction source with
                        | Some action ->
                            parse (Seq.append doc [Action (Description action)]) (Seq.skip (Seq.length action) source)
                        | None ->
                            doc
                | None ->
                    match parseAction source with
                    | Some action ->
                        parse (Seq.append doc [Action (Description action)]) (Seq.skip (Seq.length action) source)
                    | None ->
                        doc
            | '\n' ->
                parse doc (Seq.skip 1 source)
            | _ ->
                printfn "parse action"
                match parseAction source with
                | Some action ->
                    parse (Seq.append doc [Action (Description action)]) (Seq.skip (Seq.length action) source)
                | None ->
                    doc
                