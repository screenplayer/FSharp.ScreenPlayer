namespace Tests

open NUnit.Framework
open FSharp.ScreenPlayer.Lang
open System.IO

[<TestClass>]
type TestClass() =

    [<SetUp>]
    member this.Setup() = ()

    [<Test>]
    member this.TestStrValue() =
        let str = "{value}"
        let source = { chars = str; line = 0; offset = 0 }

        match parseValue source with
        | Ok (value, newSource) ->
            Assert.AreEqual(Str "value", value)
            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(7, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestListValue() =
        let str = "{valueA; valueB}"
        let source = { chars = str; line = 0; offset = 0 }

        match parseValue source with
        | Ok (value, newSource) ->
            Assert.AreEqual(List [| "valueA"; "valueB" |], value)
            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(16, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestInvalidValue() =
        let str = "{value"
        let source = { chars = str; line = 0; offset = 0 }

        match parseValue source with
        | Ok (value, _) -> Assert.AreEqual(true, false)
        | Error error ->
            Assert.AreEqual(0, error.offset)
            Assert.AreEqual(0, error.line)
            Assert.AreEqual("expected }, but found nothing", error.message)

    [<Test>]
    member this.TestStrDefinition() =
        let str = "${key}={value}"
        let source = { chars = str; line = 0; offset = 0 }

        match parseDefinition source with
        | Ok (definition, newSource) ->
            Assert.AreEqual("key", definition.name)
            Assert.AreEqual(Str "value", definition.value)
            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(14, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestListDefinition() =
        let str = "${key}={valueA; valueB}"
        let source = { chars = str; line = 0; offset = 0 }

        match parseDefinition source with
        | Ok (definition, newSource) ->
            Assert.AreEqual("key", definition.name)
            Assert.AreEqual(List [| "valueA"; "valueB" |], definition.value)
            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(23, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestInvalidDefinition() =
        let str = "${key}{value}"
        let source = { chars = str; line = 0; offset = 0 }

        match parseDefinition source with
        | Ok (value, _) -> Assert.AreEqual(true, false)
        | Error error ->
            Assert.AreEqual(6, error.offset)
            Assert.AreEqual(0, error.line)
            Assert.AreEqual("expected =, but found {", error.message)

    [<Test>]
    member this.TestSceneHeading() =
        let str = "[INT]"
        let source = { chars = str; line = 0; offset = 0 }

        match parseSceneHeading source with
        | Ok (value, newSource) ->
            Assert.AreEqual(Interior "", value)
            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(5, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestSceneHeadingWithDetails() =
        let str = "[INT - Hello]"
        let source = { chars = str; line = 0; offset = 0 }

        match parseSceneHeading source with
        | Ok (value, newSource) ->
            Assert.AreEqual(Interior "Hello", value)
            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(13, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestNestedSceneHeading() =
        let str = "[INT/EXT]"
        let source = { chars = str; line = 0; offset = 0 }

        match parseSceneHeading source with
        | Ok (value, newSource) ->
            Assert.AreEqual(Nested [| Interior ""; Exterior "" |], value)

            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(9, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestInvalidSceneHeading() =
        let str = "[INT"
        let source = { chars = str; line = 0; offset = 0 }

        match parseSceneHeading source with
        | Ok (value, _) -> Assert.AreEqual(true, false)
        | Error error ->
            Assert.AreEqual(0, error.offset)
            Assert.AreEqual(0, error.line)
            Assert.AreEqual("expected ], but found nothing", error.message)

    [<Test>]
    member this.TestAnnotation() =
        let str = "(this is annotation)"
        let source = { chars = str; line = 0; offset = 0 }

        match parseAnnotation source with
        | Ok (value, newSource) ->
            Assert.AreEqual(Annotation "this is annotation", value)
            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(20, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestInvalidAnnotation() =
        let str = "(this is invalid annotation"
        let source = { chars = str; line = 0; offset = 0 }

        match parseAnnotation source with
        | Ok (value, _) -> Assert.AreEqual(true, false)
        | Error error ->
            Assert.AreEqual(0, error.offset)
            Assert.AreEqual(0, error.line)
            Assert.AreEqual("expected ), but found nothing", error.message)

    [<Test>]
    member this.TestSingleCharacter() =
        let str = "@{charA}"
        let source = { chars = str; line = 0; offset = 0 }

        match parseCharacters source with
        | Ok (characters, newSource) ->
            Assert.AreEqual(1, Array.length characters)
            Assert.AreEqual({ name = "charA"; annotation = None }, characters.[0])
            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(8, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestGroupCharaters() =
        let str = "@{charA; charB}"
        let source = { chars = str; line = 0; offset = 0 }

        match parseCharacters source with
        | Ok (characters, newSource) ->
            Assert.AreEqual(2, Array.length characters)
            Assert.AreEqual({ name = "charA"; annotation = None }, characters.[0])
            Assert.AreEqual({ name = "charB"; annotation = None }, characters.[1])
            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(15, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestCharaterWithAnnotation() =
        let str = "@{charA; charB}(annotation)"
        let source = { chars = str; line = 0; offset = 0 }

        match parseCharacters source with
        | Ok (characters, newSource) ->
            Assert.AreEqual(2, Array.length characters)

            Assert.AreEqual(
                { name = "charA"
                  annotation = Some(Annotation "annotation") },
                characters.[0]
            )

            Assert.AreEqual(
                { name = "charB"
                  annotation = Some(Annotation "annotation") },
                characters.[1]
            )

            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(27, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestInvalidCharacter() =
        let str = "@{value"
        let source = { chars = str; line = 0; offset = 0 }

        match parseCharacters source with
        | Ok (value, _) -> Assert.AreEqual(true, false)
        | Error error ->
            Assert.AreEqual(1, error.offset)
            Assert.AreEqual(0, error.line)
            Assert.AreEqual("expected }, but found nothing", error.message)

    [<Test>]
    member this.TestDialogueText() =
        let str = "\"dialogue content\""
        let source = { chars = str; line = 0; offset = 0 }

        match parseDialogueContent [||] source with
        | Ok (value, newSource) ->
            Assert.AreEqual([| DialogueText "dialogue content" |], value)
            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(18, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestDialogueContent() =
        let str =
            "\"dialogue content\"(pause)\"dialogue content\""

        let source = { chars = str; line = 0; offset = 0 }

        match parseDialogueContent [||] source with
        | Ok (value, newSource) ->
            Assert.AreEqual(
                [| DialogueText "dialogue content"
                   DialogueAnnotation(Annotation "pause")
                   DialogueText "dialogue content" |],
                value
            )

            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(43, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestInvalidDialogueContent() =
        let str = "\"value"
        let source = { chars = str; line = 0; offset = 0 }

        match parseDialogueContent [||] source with
        | Ok (value, _) -> Assert.AreEqual(true, false)
        | Error error ->
            Assert.AreEqual(0, error.offset)
            Assert.AreEqual(0, error.line)
            Assert.AreEqual("expected \", but found nothing", error.message)

    [<Test>]
    member this.TestDialogue() =
        let str = "@{John} \"dialogue content\""
        let source = { chars = str; line = 0; offset = 0 }

        match parseDialogue [||] source with
        | Ok (value, newSource) ->
            Assert.AreEqual(
                [| { characters = [| { name = "John"; annotation = None } |]
                     contents = [| DialogueText "dialogue content" |] } |],
                value
            )

            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(26, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestMultipleCharacterDialogue() =
        let str = "@{John; Henry} \"dialogue content\""
        let source = { chars = str; line = 0; offset = 0 }

        match parseDialogue [||] source with
        | Ok (value, newSource) ->
            Assert.AreEqual(
                [| { characters =
                         [| { name = "John"; annotation = None }
                            { name = "Henry"; annotation = None } |]
                     contents = [| DialogueText "dialogue content" |] } |],
                value
            )

            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(33, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestMultipleDialogues() =
        let str =
            "@{John; Henry} \"dialogue content\" & @{John; Henry} \"dialogue content\""

        let source = { chars = str; line = 0; offset = 0 }

        match parseDialogue [||] source with
        | Ok (value, newSource) ->
            Assert.AreEqual(
                [| { characters =
                         [| { name = "John"; annotation = None }
                            { name = "Henry"; annotation = None } |]
                     contents = [| DialogueText "dialogue content" |] }
                   { characters =
                         [| { name = "John"; annotation = None }
                            { name = "Henry"; annotation = None } |]
                     contents = [| DialogueText "dialogue content" |] } |],
                value
            )

            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(69, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestTransition() =
        let str = "> CUT TO"
        let source = { chars = str; line = 0; offset = 0 }

        match parseTransition source with
        | Ok (value, newSource) ->
            Assert.AreEqual("CUT TO", value.name)
            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(8, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestInvalidTransition() =
        let str = "value"
        let source = { chars = str; line = 0; offset = 0 }

        match parseTransition source with
        | Ok (value, _) -> Assert.AreEqual(true, false)
        | Error error ->
            Assert.AreEqual(0, error.offset)
            Assert.AreEqual(0, error.line)
            Assert.AreEqual("expected >, but found v", error.message)

    [<Test>]
    member this.TestAction() =
        let str = "this is action"
        let source = { chars = str; line = 0; offset = 0 }

        match parseAction source with
        | Ok (value, newSource) ->
            Assert.AreEqual(Description "this is action", value)
            Assert.AreEqual(true, Seq.isEmpty newSource.chars)
            Assert.AreEqual(14, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestMultipleActions() =
        let str =
            """this is action"""
            + "\n"
            + """this is another action"""

        let source = { chars = str; line = 0; offset = 0 }

        match parseAction source with
        | Ok (value, newSource) ->
            Assert.AreEqual(Description "this is action", value)
            Assert.AreEqual(false, Seq.isEmpty newSource.chars)
            Assert.AreEqual(14, newSource.offset)
            Assert.AreEqual(0, newSource.line)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestMultipleStatements() =
        let str = """
> CUT TO
@{Henry} "Hello"
@{John} "Hello"

this is a action

@{Henry} "Hello"
            """
        let source = { chars = str; line = 0; offset = 0 }

        match parse [||] source with
        | Ok lines -> Assert.AreEqual(14, Seq.length lines)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestInvalidMultipleStatements() =
        let str = """
> CUT TO
@{Henry} "Hello"
@{John} "Hello"

this is a action

@{Henry} "Hello
            """
        let source = { chars = str; line = 0; offset = 0 }

        match parse [||] source with
        | Ok lines -> Assert.AreEqual(14, Seq.length lines)
        | Error error ->
            Assert.AreEqual(71, error.offset)
            Assert.AreEqual(7, error.line)
            Assert.AreEqual("expected \", but found nothing", error.message)