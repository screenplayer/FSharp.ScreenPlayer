# FSharp.ScreenPlayer
Player for ScreenPlay language written with F#

The whole solution is build with F# on .Net 5.0, make sure you have install the runtime before you run these projects.

## Solution Structure

1. FSharp.ScreenPlayer.Lang

    this projects contains code for parsing screenplay files, which format is specified in [english documents](./docs/standard/standard.en-US.md) or [chinese documents](./docs/standard/standard.zh-CN.md)

2. FSharp.ScreenPlayer.Player.Console

    this projects containes code for an console application which can print out the output of FSharp.ScreenPlayer.Lang

2. FSharp.ScreenPlayer.Pdf

    this projects containes code for an console application which can transform the result of FSharp.ScreenPlayer.Lang to an PDF file. (only support Windows system now)