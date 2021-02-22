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
        let source = { chars = str; offset = 0 }

        match parseValue source with
        | Ok (value, _) -> Assert.AreEqual(Str "value", value)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestListValue() =
        let str = "{valueA; valueB}"
        let source = { chars = str; offset = 0 }

        match parseValue source with
        | Ok (value, _) -> Assert.AreEqual(List [| "valueA"; "valueB" |], value)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestInvalidValue() =
        let str = "{value"
        let source = { chars = str; offset = 0 }

        match parseValue source with
        | Ok (value, _) -> Assert.AreEqual(true, false)
        | Error error ->
            Assert.AreEqual(0, error.offset)
            Assert.AreEqual("expected }, but found nothing", error.message)

    [<Test>]
    member this.TestStrDefinition() =
        let str = "${key}={value}"
        let source = { chars = str; offset = 0 }

        match parseDefinition source with
        | Ok (definition, _) ->
            Assert.AreEqual("key", definition.name)
            Assert.AreEqual(Str "value", definition.value)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestListDefinition() =
        let str = "${key}={valueA; valueB}"
        let source = { chars = str; offset = 0 }

        match parseDefinition source with
        | Ok (definition, _) ->
            Assert.AreEqual("key", definition.name)
            Assert.AreEqual(List [| "valueA"; "valueB" |], definition.value)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestInvalidDefinition() =
        let str = "${key}{value}"
        let source = { chars = str; offset = 0 }

        match parseDefinition source with
        | Ok (value, _) -> Assert.AreEqual(true, false)
        | Error error ->
            Assert.AreEqual(6, error.offset)
            Assert.AreEqual("expected =, but found {", error.message)

    [<Test>]
    member this.TestSceneHeading() =
        let str = "[INT]"
        let source = { chars = str; offset = 0 }

        match parseSceneHeading source with
        | Ok (value, _) -> Assert.AreEqual(Interior "INT", value)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestNestedSceneHeading() =
        let str = "[INT/EXT]"
        let source = { chars = str; offset = 0 }

        match parseSceneHeading source with
        | Ok (value, _) -> Assert.AreEqual(Nested [|Interior "INT"; Exterior "EXT"|], value)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestInvalidSceneHeading() =
        let str = "[INT"
        let source = { chars = str; offset = 0 }

        match parseSceneHeading source with
        | Ok (value, _) -> Assert.AreEqual(true, false)
        | Error error ->
            Assert.AreEqual(0, error.offset)
            Assert.AreEqual("expected ], but found nothing", error.message)

    [<Test>]
    member this.TestAnnotation() =
        let str = "(this is annotation)"
        let source = { chars = str; offset = 0 }

        match parseAnnotation source with
        | Ok (value, _) -> Assert.AreEqual(Annotation "this is annotation", value)
        | _ -> Assert.AreEqual(true, false)

    [<Test>]
    member this.TestInvalidAnnotation() =
        let str = "(this is invalid annotation"
        let source = { chars = str; offset = 0 }

        match parseAnnotation source with
        | Ok (value, _) -> Assert.AreEqual(true, false)
        | Error error ->
            Assert.AreEqual(0, error.offset)
            Assert.AreEqual("expected ), but found nothing", error.message)