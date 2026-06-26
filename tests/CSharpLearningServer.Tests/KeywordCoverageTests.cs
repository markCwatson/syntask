namespace CSharpLearningServer.Tests;

/// <summary>
/// Tests every supported keyword produces a non-empty hover
/// when placed in valid syntax context.
/// </summary>
public class KeywordCoverageTests
{
  public static IEnumerable<object[]> KeywordSamples => new List<object[]>
    {
        new object[] { "abstract class X {}", "abstract" },
        new object[] { "sealed class X {}", "sealed" },
        new object[] { "class X { public virtual void M() {} }", "virtual" },
        new object[] { "class X : Y { public override void M() {} }", "override" },
        new object[] { "class X { async Task M() { } }", "async" },
        new object[] { "class X { async Task M() { await Task.Delay(1); } }", "await" },
        new object[] { "class X { public required string Name { get; init; } }", "required" },
        new object[] { "class X { public string Name { get; init; } }", "init" },
        new object[] { "record Person(string Name);", "record" },
        new object[] { "partial class X {}", "partial" },
        new object[] { "public class X {}", "public" },
        new object[] { "class X { private int _x; }", "private" },
        new object[] { "class X : Y { protected void M() {} }", "protected" },
        new object[] { "internal class X {}", "internal" },
        new object[] { "static class X {}", "static" },
        new object[] { "class X { readonly int _x = 1; }", "readonly" },
        new object[] { "class X { const int Y = 1; }", "const" },
        new object[] { "class X {}", "class" },
        new object[] { "struct Point { public int X; }", "struct" },
        new object[] { "interface IFoo { void Do(); }", "interface" },
        new object[] { "enum Color { Red, Green }", "enum" },
        new object[] { "namespace Foo;", "namespace" },
        new object[] { "using System;", "using" },
        new object[] { "class X { IEnumerable<int> M() { yield return 1; } }", "yield" },
        new object[] { "class X { void M(object o) { if (o is string s) {} } }", "is" },
        new object[] { "class X { void M(object o) { var s = o as string; } }", "as" },
        new object[] { "class X { void M() { var x = new X(); } }", "new" },
        new object[] { "class X { int M() { return 1; } }", "return" },
        new object[] { "class X { volatile bool _flag; }", "volatile" },
        new object[] { "class X { extern static void M(); }", "extern" },
        new object[] { "unsafe class X {}", "unsafe" },
        new object[] { "class X<T> where T : class {}", "where" },
        new object[] { "class X { void M(ref int x) {} }", "ref" },
        new object[] { "class X { void M(out int x) { x = 0; } }", "out" },
        new object[] { "class X { void M(params int[] x) {} }", "params" },
        new object[] { "class X { int _x; X(int x) { this._x = x; } }", "this" },
        new object[] { "class X : Y { X() : base() {} }", "base" },
        new object[] { "class X { void M() { lock(this) {} } }", "lock" },
        new object[] { "delegate void Handler();", "delegate" },
        new object[] { "class X { event EventHandler E; }", "event" },
    };

  [Theory]
  [MemberData(nameof(KeywordSamples))]
  public void AllSupportedKeywords_ProduceHover(string code, string keyword)
  {
    var result = Hover.ForToken(code, keyword);

    Assert.NotNull(result.Markdown);
    Assert.True(result.Markdown.Length > 10,
        $"Hover for '{keyword}' was too short: '{result.Markdown}'");
  }

  [Theory]
  [MemberData(nameof(KeywordSamples))]
  public void AllSupportedKeywords_IncludeRange(string code, string keyword)
  {
    var result = Hover.ForToken(code, keyword);

    Assert.NotNull(result.Range);
    Assert.True(result.Range.EndCharacter > result.Range.StartCharacter ||
                result.Range.EndLine > result.Range.StartLine,
        $"Range for '{keyword}' has zero width.");
  }
}
