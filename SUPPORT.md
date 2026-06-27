# Support

## Getting Help

- **Issues**: File a bug or feature request on [GitHub Issues](https://github.com/markCwatson/syntask/issues)
- **Discussions**: Ask questions in [GitHub Discussions](https://github.com/markCwatson/syntask/discussions)

## Common Issues

### Hovers not appearing

1. Ensure `syntask.enabled` is `true` in settings
2. Verify .NET SDK is installed: `dotnet --version`
3. Check the "syntask" output channel (View → Output → select "syntask")

### Server fails to start

- Requires .NET 10.0+ SDK on PATH
- On macOS, ensure `/usr/local/share/dotnet` or `~/.dotnet` is in your PATH

### Conflicting with C# extension hovers

- syntask adds a second hover panel beneath the C# extension's hover. Both appear side-by-side. If you prefer command-only mode, set `syntask.triggerMode` to `commandOnly`.
