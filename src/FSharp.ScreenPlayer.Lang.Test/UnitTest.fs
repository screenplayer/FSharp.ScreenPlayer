namespace Tests

open NUnit.Framework
open FSharp.ScreenPlayer.Lang
open System.IO

[<TestClass>]
type TestClass () =

    [<SetUp>]
    member this.Setup () =
        ()

    [<Test>]
    member this.TestDefinition () =
        let str = "${key}={value}"
        let doc = Seq.empty
        let res = parse doc str
        Assert.AreEqual(Seq.length res, 1)
        let definition = Seq.head res
        match definition with
        | Definition (key, value) ->
            Assert.AreEqual(key, "{key}")
            Assert.AreEqual(value, "{value}")
        | _ ->
            Assert.AreEqual(true, false)

    [<Test>]
    member this.TestMultipleLineDefinition () =
        let str =
            "${Contact}={
                mrchangji@outlook.com
                https://github.com/ScreenPlayer/lang.git
            }"
        let doc = Seq.empty
        let res = parse doc str
        Assert.AreEqual(Seq.length res, 1)
        let definition = Seq.head res
        match definition with
        | Definition (key, value) ->
            Assert.AreEqual(key, "{Contact}")
            Assert.AreEqual(value, "{
                mrchangji@outlook.com
                https://github.com/ScreenPlayer/lang.git
            }")
        | _ ->
            Assert.AreEqual(true, false)

    [<Test>]
    member this.TestHeading () =
        let str = "[INT]"
        let doc = Seq.empty
        let res = parse doc str
        Assert.AreEqual(Seq.length res, 1)
        let sceneHeading = Seq.head res
        match sceneHeading with
        | SceneHeading heading ->
            Assert.AreEqual(heading, "[INT]")
        | _ ->
            Assert.AreEqual(true, false)
    [<Test>]
    member this.TestDialogue () =
        let str = "@{Hello} \"hello, world\""
        let doc = Seq.empty
        let res = parse doc str
        Assert.AreEqual(Seq.length res, 1)
        let dialogue = Seq.head res
        match dialogue with
        | Dialogue (character, content) ->
            Assert.AreEqual(character.name, "{Hello}")
            Assert.AreEqual(true, Option.isNone character.parenthetical)
            Assert.AreEqual(content, "\"hello, world\"")
        | _ ->
            Assert.AreEqual(true, false)

    [<Test>]
    member this.TestParentheticalDialogue () =
        let str = "@{Hello}(parenthetical content) \"hello, world\""
        let doc = Seq.empty
        let res = parse doc str
        Assert.AreEqual(Seq.length res, 1)
        let dialogue = Seq.head res
        match dialogue with
        | Dialogue (character, content) ->
            Assert.AreEqual(character.name, "{Hello}")
            Assert.AreEqual(true, Option.isSome character.parenthetical)
            Assert.AreEqual("(parenthetical content)", character.parenthetical.Value)
            Assert.AreEqual(content, "\"hello, world\"")
        | _ ->
            Assert.AreEqual(true, false)

    [<Test>]
    member this.TestAction () =
        let str = "it's a normal action"
        let doc = Seq.empty
        let res = parse doc str
        Assert.AreEqual(Seq.length res, 1)
        let action = Seq.head res
        match action with
        | Action (Description content) ->
            Assert.AreEqual(content, "it's a normal action")
        | _ ->
            Assert.AreEqual(true, false)

    [<Test>]
    member this.TestIntegration () =
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
        Assert.AreEqual(7, Seq.length doc)
