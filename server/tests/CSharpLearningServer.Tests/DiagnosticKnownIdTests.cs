namespace CSharpLearningServer.Tests;

/// <summary>
/// Tests for known diagnostic ID explanations.
/// Each test triggers a real compiler diagnostic and verifies
/// the curated explanation contains the expected content.
/// </summary>
public class DiagnosticKnownIdTests
{
  [Fact]
  public void CS0103_UndefinedName_ExplainsNameNotFound()
  {
    var code = """
        class X
        {
            void M() { int y = foo; }
        }
        """;
    var result = Diag.ForToken(code, "foo");

    Assert.NotNull(result.Markdown);
    Assert.Contains("CS0103", result.Markdown);
    Assert.Contains("Name not found", result.Markdown);
    Assert.Contains("Common fixes", result.Markdown);
  }

  [Fact]
  public void CS1002_MissingSemicolon_ExplainsSemicolonExpected()
  {
    // Missing semicolon after "int x = 5"
    var code = """
        class X
        {
            void M() { int x = 5 }
        }
        """;
    // Cursor on '5' — the diagnostic is emitted nearby
    var result = Diag.ForToken(code, "5");

    Assert.NotNull(result.Markdown);
    Assert.Contains("CS1002", result.Markdown);
    Assert.Contains("Semicolon expected", result.Markdown);
  }

  [Fact]
  public void CS0246_UnknownType_ExplainsTypeNotFound()
  {
    var code = """
        class X
        {
            void M() { Foobar x = null; }
        }
        """;
    var result = Diag.ForToken(code, "Foobar");

    Assert.NotNull(result.Markdown);
    Assert.Contains("CS0246", result.Markdown);
    Assert.Contains("Type or namespace not found", result.Markdown);
    Assert.Contains("using", result.Markdown); // related concept
  }

  [Fact]
  public void CS0029_TypeConversion_ExplainsImplicitConversion()
  {
    var code = """
        class X
        {
            void M() { int count = "hello"; }
        }
        """;
    var result = Diag.ForToken(code, "hello");

    Assert.NotNull(result.Markdown);
    Assert.Contains("CS0029", result.Markdown);
    Assert.Contains("Cannot implicitly convert", result.Markdown);
  }

  [Fact]
  public void CS0161_NotAllPathsReturn_ExplainsReturnPaths()
  {
    var code = """
        class X
        {
            int M(bool b)
            {
                if (b) return 1;
            }
        }
        """;
    var result = Diag.ForToken(code, "M(");

    Assert.NotNull(result.Markdown);
    Assert.Contains("CS0161", result.Markdown);
    Assert.Contains("Not all code paths return", result.Markdown);
    Assert.Contains("return", result.Markdown); // related concept
  }

  [Fact]
  public void CS0165_UnassignedVariable_ExplainsDefiniteAssignment()
  {
    var code = """
        class X
        {
            void M()
            {
                int result;
                System.Console.WriteLine(result);
            }
        }
        """;
    var result = Diag.ForToken(code, "result);");

    Assert.NotNull(result.Markdown);
    Assert.Contains("CS0165", result.Markdown);
    Assert.Contains("unassigned local variable", result.Markdown);
  }

  [Fact]
  public void CS0266_ExplicitConversion_ExplainsCastRequired()
  {
    var code = """
        class X
        {
            void M() { int x = 3.14; }
        }
        """;
    var result = Diag.ForToken(code, "3.14");

    Assert.NotNull(result.Markdown);
    Assert.Contains("CS0266", result.Markdown);
    Assert.Contains("explicit", result.Markdown.ToLower());
  }

  [Fact]
  public void CS0120_NonStaticFromStatic_ExplainsObjectReference()
  {
    var code = """
        class X
        {
            int count = 0;
            static void Main()
            {
                System.Console.WriteLine(count);
            }
        }
        """;
    var result = Diag.ForToken(code, "count);");

    Assert.NotNull(result.Markdown);
    Assert.Contains("CS0120", result.Markdown);
    Assert.Contains("static", result.Markdown); // related concept
  }

  [Fact]
  public void CS0535_MissingInterfaceMember_ExplainsImplementation()
  {
    var code = """
        interface IShape { double Area(); }
        class Circle : IShape { }
        """;
    var result = Diag.ForToken(code, "Circle");

    Assert.NotNull(result.Markdown);
    Assert.Contains("CS0535", result.Markdown);
    Assert.Contains("implement", result.Markdown.ToLower());
    Assert.Contains("interface", result.Markdown); // related concept
  }

  [Fact]
  public void CS0534_MissingAbstractMember_ExplainsOverrideRequired()
  {
    var code = """
        abstract class Shape { public abstract double Area(); }
        class Circle : Shape { }
        """;
    var result = Diag.ForToken(code, "Circle");

    Assert.NotNull(result.Markdown);
    Assert.Contains("CS0534", result.Markdown);
    Assert.Contains("abstract", result.Markdown); // related concept
  }

  [Fact]
  public void CS0115_NoMethodToOverride_ExplainsOverrideMismatch()
  {
    var code = """
        class Base { }
        class Derived : Base
        {
            public override void DoStuff() { }
        }
        """;
    var result = Diag.ForToken(code, "DoStuff");

    Assert.NotNull(result.Markdown);
    Assert.Contains("CS0115", result.Markdown);
    Assert.Contains("override", result.Markdown); // related concept
  }

  [Fact]
  public void CS0106_InvalidModifier_ExplainsModifierRule()
  {
    // 'volatile' is not valid on a method — triggers CS0106
    var code = """
        class X
        {
            volatile void M() { }
        }
        """;
    var result = Diag.ForToken(code, "volatile");

    Assert.NotNull(result.Markdown);
    Assert.Contains("CS0106", result.Markdown);
    Assert.Contains("Modifier not valid", result.Markdown);
  }

  [Fact]
  public void CS1061_MissingMember_ExplainsNoDefinition()
  {
    var code = """
        class X
        {
            void M() { "hello".Foo(); }
        }
        """;
    var result = Diag.ForToken(code, "Foo");

    Assert.NotNull(result.Markdown);
    Assert.Contains("CS1061", result.Markdown);
    Assert.Contains("does not contain a definition", result.Markdown);
  }

  [Fact]
  public void CS1503_ArgumentMismatch_ExplainsTypeMismatch()
  {
    var code = """
        class X
        {
            void Take(int n) { }
            void M() { Take("hello"); }
        }
        """;
    var result = Diag.ForToken(code, "hello");

    Assert.NotNull(result.Markdown);
    Assert.Contains("CS1503", result.Markdown);
    Assert.Contains("Argument type mismatch", result.Markdown);
  }
}
