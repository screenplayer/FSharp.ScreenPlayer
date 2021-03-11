# ScreenPlay 语言标准（非正式）

## 简介

ScreenPlay是一门剧本编写的语言，使用纯文本格式，易于机器解析。

## 语法

### 定义

编写剧本时，作者可以定义开始，例如标题、作者名字、撰写日期等。在ScreenPlay中，定义的格式为 *${键}={值}* ，定义的值可以为字符串或者使用 **;** 分割的多个字符串。

    ${Title}={Document of ScreenPlay}
    ${Credit}={Written by}
    ${Author}={Jacob Chang}
    ${Draft Date}={06/20/2019}
    ${Contact}={
        https://github.com/screenplayer/FSharp.ScreenPlayer
    }
    ${Characters}={Character A; Character B}

### 场景标题

场景标题使用方括号包裹的字符串表示。用 **-** 添加场景描述， **/** 表示嵌套场景，例如 EXT/INT 代表室外的某种内景，例如室外的汽车内。

    [EXT - 车里]
    [INT - 湖上]
    [EST]
    [EXT/INT]
    [INT/EXT]

### 角色

角色引用使用 **@{** 开头，使用 **}** 结尾，类似于社交媒体上的引用。

    @{Character}

### 对话

对话以发言角色 **@{CharacterA}** 开头，发言内容使用双引号 **"** 括起来。对于同时对话，可以使用 **&** 合并多个对话或者使用 **;** 合并多个角色。

    @{Character A} "Hello, B"
    @{Character B} "Hello, A"

    @{Character A} "Hey, B" & @{Character B} "Hey A"
    @{Character A; Character B} "Morning"

### 标注

标注使用括号进行标记，以 **(** 开头，以 **)** 结尾。

    @{Character}(with sad face) "Long time no see" (pause) "Where have you been"

### 场景切换

场景切换使用 **>** 标记

    > CUT TO:

### 行为

剩余内容将被视为行为。
