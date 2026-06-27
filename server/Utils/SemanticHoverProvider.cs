using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Server.Models;

namespace Server.Utils;

/// <summary>
/// Provides hover explanations for tokens that require semantic analysis
/// (type inference, resolved types, etc.).
/// </summary>
public static class SemanticHoverProvider
{
  public static HoverResponse GetHover(
      SyntaxToken token,
      SyntaxTree tree,
      SourceText sourceText,
      HoverRequest request)
  {
    // Only attempt semantic analysis for specific identifiers/keywords
    if (!NeedsSemanticAnalysis(token))
    {
      return new HoverResponse(null, null);
    }

    var model = SemanticAnalyzer.GetSemanticModel(tree);
    if (model is null)
    {
      return new HoverResponse(null, null);
    }

    var markdown = token.Text switch
    {
      "var" => ExplainVar(token, model, request),
      "dynamic" => ExplainDynamic(token, request),
      "default" => ExplainDefault(token, model, request),
      "nameof" => ExplainNameof(token, model, request),
      "typeof" => ExplainTypeof(token, model, request),
      _ => null
    };

    if (markdown is null)
    {
      return new HoverResponse(null, null);
    }

    return new HoverResponse(markdown, ToRange(token, sourceText));
  }

  private static bool NeedsSemanticAnalysis(SyntaxToken token)
  {
    return token.Text is "var" or "dynamic" or "default" or "nameof" or "typeof";
  }

  private static string? ExplainVar(SyntaxToken token, SemanticModel model, HoverRequest request)
  {
    // var must be in a local declaration or foreach
    var node = token.Parent;

    if (node is IdentifierNameSyntax identName)
    {
      // Check if this is actually a type named "var" used as var keyword
      if (identName.Parent is VariableDeclarationSyntax varDecl)
      {
        var typeInfo = model.GetTypeInfo(varDecl.Type);
        var resolvedType = typeInfo.Type;

        if (resolvedType is not null && resolvedType.Kind != SymbolKind.ErrorType)
        {
          var typeName = resolvedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
          return $"""
              **`var`** → `{typeName}`

              The compiler infers the type from the right-hand side of the assignment. `var` is purely a convenience — the compiled code is identical to writing `{typeName}` explicitly.

              ```csharp
              var x = expression;  // compiler sees: {typeName} x = expression;
              ```

              Use `var` when the type is obvious from context. Use an explicit type when clarity matters.
              """;
        }

        // Type couldn't be resolved (likely missing references)
        return """
            **`var`** (implicit typing)

            The compiler infers the type from the right-hand side. The variable is still strongly typed — `var` does not mean "dynamic" or "untyped."

            ```csharp
            var name = "hello";  // compiler infers: string name
            var list = new List<int>();  // compiler infers: List<int> list
            ```

            Use `var` when the type is obvious from context. Use an explicit type when clarity matters.
            """;
      }

      if (identName.Parent is ForEachStatementSyntax forEach)
      {
        var typeInfo = model.GetTypeInfo(forEach.Type);
        var resolvedType = typeInfo.Type;

        if (resolvedType is not null && resolvedType.Kind != SymbolKind.ErrorType)
        {
          var typeName = resolvedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
          return $"""
              **`var`** → `{typeName}`

              In this `foreach`, the compiler infers each element is `{typeName}`.

              ```csharp
              foreach (var item in collection)  // item is {typeName}
              ```
              """;
        }
      }
    }

    return null;
  }

  private static string ExplainDynamic(SyntaxToken token, HoverRequest request)
  {
    return """
        **`dynamic`**

        Bypasses compile-time type checking. Operations on a `dynamic` variable are resolved at runtime.

        ```csharp
        dynamic obj = GetSomething();
        obj.DoWork();  // no compile error even if DoWork doesn't exist
        ```

        Use sparingly — you lose IntelliSense, refactoring support, and compile-time safety. Commonly used for COM interop or deserializing loosely-typed data.

        Unlike `var`, `dynamic` is a real type (System.Object with dynamic dispatch). Unlike `object`, you can call any member without casting.
        """;
  }

  private static string? ExplainDefault(SyntaxToken token, SemanticModel model, HoverRequest request)
  {
    var node = token.Parent;

    if (node is DefaultExpressionSyntax defaultExpr)
    {
      // default(T) form
      var typeInfo = model.GetTypeInfo(defaultExpr);
      var resolvedType = typeInfo.Type;

      if (resolvedType is not null && resolvedType.Kind != SymbolKind.ErrorType)
      {
        var typeName = resolvedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var defaultValue = GetDefaultValueDescription(resolvedType);
        return $"""
            **`default({typeName})`** → `{defaultValue}`

            Produces the default value for type `{typeName}`.

            For value types, `default` is zero/false/empty. For reference types, `default` is `null`.
            """;
      }
    }

    if (node is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.DefaultLiteralExpression } defaultLiteral)
    {
      // default literal (C# 7.1+)
      var typeInfo = model.GetTypeInfo(defaultLiteral);
      var resolvedType = typeInfo.ConvertedType;

      if (resolvedType is not null && resolvedType.Kind != SymbolKind.ErrorType)
      {
        var typeName = resolvedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var defaultValue = GetDefaultValueDescription(resolvedType);
        return $"""
            **`default`** → `{defaultValue}` (inferred type: `{typeName}`)

            The `default` literal infers its type from context. Equivalent to `default({typeName})`.

            ```csharp
            int x = default;     // 0
            string s = default;  // null
            bool b = default;    // false
            ```
            """;
      }
    }

    // Generic fallback
    return """
        **`default`**

        Produces the default value for a type: `0` for numbers, `false` for bool, `null` for reference types, and zeroed structs.

        ```csharp
        int x = default;     // 0
        string s = default;  // null
        bool b = default;    // false
        DateTime d = default; // 0001-01-01
        ```
        """;
  }

  private static string? ExplainNameof(SyntaxToken token, SemanticModel model, HoverRequest request)
  {
    var node = token.Parent;

    // nameof is an InvocationExpression with identifier "nameof"
    if (node is IdentifierNameSyntax { Identifier.Text: "nameof" } &&
        node.Parent is InvocationExpressionSyntax invocation)
    {
      var argument = invocation.ArgumentList.Arguments.FirstOrDefault();
      if (argument is not null)
      {
        var argText = argument.ToString();
        // nameof resolves to the last identifier in the expression
        var lastDot = argText.LastIndexOf('.');
        var result = lastDot >= 0 ? argText[(lastDot + 1)..] : argText;

        return $"""
            **`nameof({argText})`** → `"{result}"`

            Produces the name of a symbol as a string at compile time. Unlike a string literal, `nameof` is refactoring-safe — renaming the symbol updates this automatically.

            ```csharp
            nameof({argText})  // "{result}"
            ```

            Common uses: argument validation (`ArgumentNullException`), `INotifyPropertyChanged`, logging.
            """;
      }
    }

    return """
        **`nameof`**

        Returns the name of a variable, type, or member as a compile-time string constant.

        ```csharp
        nameof(MyClass)       // "MyClass"
        nameof(obj.Property)  // "Property"
        ```

        Unlike hardcoded strings, `nameof` is refactoring-safe and checked at compile time.
        """;
  }

  private static string? ExplainTypeof(SyntaxToken token, SemanticModel model, HoverRequest request)
  {
    var node = token.Parent;

    if (node is TypeOfExpressionSyntax typeofExpr)
    {
      var typeInfo = model.GetTypeInfo(typeofExpr.Type);
      var resolvedType = typeInfo.Type;

      if (resolvedType is not null && resolvedType.Kind != SymbolKind.ErrorType)
      {
        var typeName = resolvedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        return $"""
            **`typeof({typeName})`** → `System.Type`

            Returns the `System.Type` object for `{typeName}` at compile time. Used for reflection, serialization, and type comparisons.

            ```csharp
            Type t = typeof({typeName});
            Console.WriteLine(t.FullName);
            ```

            Unlike `obj.GetType()`, `typeof` is resolved at compile time and doesn't need an instance.
            """;
      }
    }

    return """
        **`typeof`**

        Returns the `System.Type` metadata object for a type, resolved at compile time.

        ```csharp
        Type t = typeof(string);  // System.String
        Type t2 = typeof(List<>); // open generic
        ```

        Unlike `GetType()` (which needs an instance), `typeof` works on the type name directly.
        """;
  }

  private static string GetDefaultValueDescription(ITypeSymbol type)
  {
    return type.SpecialType switch
    {
      SpecialType.System_Boolean => "false",
      SpecialType.System_Byte or SpecialType.System_SByte => "0",
      SpecialType.System_Int16 or SpecialType.System_UInt16 => "0",
      SpecialType.System_Int32 or SpecialType.System_UInt32 => "0",
      SpecialType.System_Int64 or SpecialType.System_UInt64 => "0",
      SpecialType.System_Single => "0.0f",
      SpecialType.System_Double => "0.0",
      SpecialType.System_Decimal => "0m",
      SpecialType.System_Char => "'\\0'",
      SpecialType.System_String => "null",
      _ => type.IsValueType ? "zeroed struct" : "null"
    };
  }

  private static HoverRange ToRange(SyntaxToken token, SourceText sourceText)
  {
    var start = sourceText.Lines.GetLinePosition(token.Span.Start);
    var end = sourceText.Lines.GetLinePosition(token.Span.End);

    return new HoverRange(
        start.Line,
        start.Character,
        end.Line,
        end.Character
    );
  }
}
