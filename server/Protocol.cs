using System.Text.Json;

public sealed record RpcRequest(
    int Id,
    string Method,
    JsonElement Params
);

public sealed record RpcResponse(
    int Id,
    object? Result,
    string? Error = null
);

public sealed record HoverRequest(
    string Text,
    string? FilePath,
    int Line,
    int Character,
    string DetailLevel,
    bool IncludeExamples
);

public sealed record HoverResponse(
    string? Markdown,
    HoverRange? Range
);

public sealed record HoverRange(
    int StartLine,
    int StartCharacter,
    int EndLine,
    int EndCharacter
);
