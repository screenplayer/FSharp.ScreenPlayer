namespace Tests

open NUnit.Framework
open FSharp.ScreenPlayer.Lang

[<TestClass>]
type TestClass () =

    [<SetUp>]
    member this.Setup () =
        ()

    [<Test>]
    member this.TestSplit () =
        let str = "hello world"
        let sep = " "
        let parts = split sep str
        Assert.AreEqual(Seq.length parts, 2)
        Assert.AreEqual(Seq.nth 0 parts, "hello")
        Assert.AreEqual(Seq.nth 1 parts, "world")
