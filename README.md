# syntask

**Learning hovers for C# syntax features** — context-aware explanations for keywords, modifiers, and language constructs, powered by Roslyn.

## What it does

Hover over any C# keyword and get a concise teaching explanation with examples. The extension understands syntax context: `abstract` on a class gets a different explanation than `abstract` on a method.

![Hover example](https://raw.githubusercontent.com/markCwatson/syntask/main/images/demo.gif)

## Features

- **45+ keywords covered**: `abstract`, `sealed`, `virtual`, `override`, `async`, `await`, `record`, `required`, `init`, `partial`, `static`, `readonly`, `const`, `yield`, `is`, `as`, `where`, `when`, `delegate`, `event`, and more
- **Semantic analysis**: `var` shows the inferred type, `default` shows the resolved value, `nameof` shows the string result
- **Context-aware**: same keyword → different explanation depending on syntax position
- **Configurable**: adjust detail level (beginner/intermediate/advanced), toggle examples, or use command-only mode
- **Local-only**: all analysis runs on your machine via Roslyn. No code is sent anywhere.
- **No AI or LLM!**: No LLMs were harmed during the making of this extension. But seriously, no AI/LLM is used to generate the information: it's all Roslyn-based.

## Settings

| Setting                                | Default    | Description                                          |
| -------------------------------------- | ---------- | ---------------------------------------------------- |
| `csharpLearningHovers.enabled`         | `true`     | Enable/disable learning hovers                       |
| `csharpLearningHovers.detailLevel`     | `beginner` | Detail level: beginner, intermediate, advanced       |
| `csharpLearningHovers.includeExamples` | `true`     | Show code examples in hovers                         |
| `csharpLearningHovers.triggerMode`     | `hover`    | `hover` for automatic, `commandOnly` for manual only |

## Commands

- **C#: Explain Syntax at Cursor** — opens a full explanation in a new tab

## Requirements

- [.NET SDK](https://dotnet.microsoft.com/download) (10.0 or later) on your PATH
- [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) for language support

## Privacy

This extension analyzes C# source **locally only**. It does not send code to any remote service. The Roslyn analysis runs as a local .NET process within your workspace.

## Troubleshooting

**Hovers not appearing?**

- Check that `csharpLearningHovers.enabled` is `true`
- Verify `dotnet` is on your PATH: run `dotnet --version` in a terminal
- Check the "syntask" output channel for server errors

**Slow first hover?**

- The .NET process starts on first use. Subsequent hovers are fast.

## Known Limitations

- Syntax-only for most keywords (no project/solution context)
- `var` inference requires basic type resolution; complex generics may show `?`
- Does not replace the C# extension's symbol hovers — it adds teaching content alongside them
