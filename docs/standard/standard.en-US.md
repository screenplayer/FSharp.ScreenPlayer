# ScreenPlay language standard (Informal)

## Introduction

ScreenPlay is language for writing screen play. It has plain text formatting syntax and is very esey for computer to parsing.

## Grammar

### Definiton

When writing screen play, authors can start with definitons, like title, author name, draft date and so on. In ScreenPlay, the format of definiton is *${key}={value}*. And value can be string or multiple string separate with **;**.

    ${Title}={Document of ScreenPlay}
    ${Credit}={Written by}
    ${Author}={Jacob Chang}
    ${Draft Date}={06/20/2019}
    ${Contact}={
        https://github.com/screenplayer/FSharp.ScreenPlayer
    }
    ${Characters}={Character A; Character B}

### Scene Heading

Scene Heading are strings wrapped by square brackets. Use **/** to indicate nested scene, like EXT/INT means some interior scene in outside.

    [EXT]
    [INT]
    [EST]
    [EXT/INT]
    [INT/EXT]

### Characters

Character starts with **@{** and ends with **}**, similar to what you do on social media.

    @{Character}

### Dialogue

Dialogue is started with **CharacterA**, words are surronded by **"**. For dual dialogue, you can use **&** to combine multiple dialogues or use **;** to combine multiple characters. 

    @{Character A} "Hello, B"
    @{Character B} "Hello, A"

    @{Character A} "Hey, B" & @{Character B} "Hey A"
    @{Character A; Character B} "Morning"

### Annotation

Annotation are wrapped in parentheses

    @{Character}(with sad face) "Long time no see" (pause) "Where have you been"

### Transition

Transitions are start with ">"

    > CUT TO

### Action

All other paragraphs will be treated as actions.
