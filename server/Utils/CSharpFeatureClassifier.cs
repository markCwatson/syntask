using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Server.Models;

namespace Server.Utils;

public static class CSharpFeatureClassifier
{
    public static HoverResponse GetHover(
        SyntaxToken token,
        SourceText sourceText,
        HoverRequest request)
    {
        string? markdown = token.Kind() switch
        {
            SyntaxKind.AbstractKeyword => ExplainAbstract(token, request),
            SyntaxKind.SealedKeyword => ExplainSealed(token, request),
            SyntaxKind.VirtualKeyword => ExplainVirtual(token, request),
            SyntaxKind.OverrideKeyword => ExplainOverride(token, request),
            SyntaxKind.AsyncKeyword => ExplainAsync(token, request),
            SyntaxKind.AwaitKeyword => ExplainAwait(token, request),
            SyntaxKind.RequiredKeyword => ExplainRequired(token, request),
            SyntaxKind.InitKeyword => ExplainInit(token, request),
            SyntaxKind.RecordKeyword => ExplainRecord(token, request),
            SyntaxKind.PartialKeyword => ExplainPartial(token, request),
            SyntaxKind.PublicKeyword => ExplainAccessModifier(token, request),
            SyntaxKind.PrivateKeyword => ExplainAccessModifier(token, request),
            SyntaxKind.ProtectedKeyword => ExplainAccessModifier(token, request),
            SyntaxKind.InternalKeyword => ExplainAccessModifier(token, request),
            SyntaxKind.StaticKeyword => ExplainStatic(token, request),
            SyntaxKind.ReadOnlyKeyword => ExplainReadonly(token, request),
            SyntaxKind.ConstKeyword => ExplainConst(token, request),
            SyntaxKind.ClassKeyword => ExplainClass(token, request),
            SyntaxKind.StructKeyword => ExplainStruct(token, request),
            SyntaxKind.InterfaceKeyword => ExplainInterface(token, request),
            SyntaxKind.EnumKeyword => ExplainEnum(token, request),
            SyntaxKind.NamespaceKeyword => ExplainNamespace(token, request),
            SyntaxKind.UsingKeyword => ExplainUsing(token, request),
            SyntaxKind.YieldKeyword => ExplainYield(token, request),
            SyntaxKind.IsKeyword => ExplainIs(token, request),
            SyntaxKind.AsKeyword => ExplainAs(token, request),
            SyntaxKind.NewKeyword => ExplainNew(token, request),
            SyntaxKind.ReturnKeyword => ExplainReturn(token, request),
            SyntaxKind.VolatileKeyword => ExplainVolatile(token, request),
            SyntaxKind.ExternKeyword => ExplainExtern(token, request),
            SyntaxKind.UnsafeKeyword => ExplainUnsafe(token, request),
            SyntaxKind.WhereKeyword => ExplainWhere(token, request),
            SyntaxKind.WhenKeyword => ExplainWhen(token, request),
            SyntaxKind.InKeyword => ExplainIn(token, request),
            SyntaxKind.OutKeyword => ExplainOut(token, request),
            SyntaxKind.RefKeyword => ExplainRef(token, request),
            SyntaxKind.ParamsKeyword => ExplainParams(token, request),
            SyntaxKind.ThisKeyword => ExplainThis(token, request),
            SyntaxKind.BaseKeyword => ExplainBase(token, request),
            SyntaxKind.LockKeyword => ExplainLock(token, request),
            SyntaxKind.DelegateKeyword => ExplainDelegate(token, request),
            SyntaxKind.EventKeyword => ExplainEvent(token, request),
            _ => null
        };

        if (markdown is null)
        {
            return new HoverResponse(null, null);
        }

        return new HoverResponse(markdown, ToRange(token, sourceText));
    }

    private static string ExplainAbstract(SyntaxToken token, HoverRequest request)
    {
        return token.Parent switch
        {
            ClassDeclarationSyntax => """
                **`abstract` class**

                An `abstract` class cannot be instantiated directly. It is designed to be used as a base class.

                ```csharp
                abstract class Shape
                {
                    public abstract double Area();
                }
                ```

                Common mistake: you cannot create an instance with `new` on an abstract class.
                """,

            MethodDeclarationSyntax or PropertyDeclarationSyntax => """
                **`abstract` member**

                An `abstract` member has no implementation here. A non-abstract derived class must override it.

                ```csharp
                abstract class Shape
                {
                    public abstract double Area();
                }
                ```

                Common mistake: an abstract member cannot have a body.
                """,

            _ => """
                **`abstract`**

                Marks a type or member as incomplete and intended to be completed by derived types.
                """
        };
    }

    private static string ExplainSealed(SyntaxToken token, HoverRequest request)
    {
        return """
            **`sealed`**

            Prevents a class from being inherited, or prevents a method from being further overridden.

            ```csharp
            sealed class FinalVersion { }
            ```
            """;
    }

    private static string ExplainVirtual(SyntaxToken token, HoverRequest request)
    {
        return """
            **`virtual`**

            Allows a method or property to be overridden in derived classes. The base implementation is used unless overridden.

            ```csharp
            class Animal
            {
                public virtual void Speak() => Console.WriteLine("...");
            }
            ```
            """;
    }

    private static string ExplainOverride(SyntaxToken token, HoverRequest request)
    {
        return """
            **`override`**

            Provides a new implementation of a `virtual` or `abstract` member inherited from a base class.

            ```csharp
            class Dog : Animal
            {
                public override void Speak() => Console.WriteLine("Woof");
            }
            ```
            """;
    }

    private static string ExplainAsync(SyntaxToken token, HoverRequest request)
    {
        return """
            **`async`**

            Marks a method as asynchronous. It enables the use of `await` inside the method and causes it to return a `Task` or `Task<T>`.

            ```csharp
            async Task<string> FetchDataAsync()
            {
                var result = await httpClient.GetStringAsync(url);
                return result;
            }
            ```
            """;
    }

    private static string ExplainAwait(SyntaxToken token, HoverRequest request)
    {
        return """
            **`await`**

            Suspends execution of the method until the awaited task completes, without blocking the thread.

            ```csharp
            var data = await FetchDataAsync();
            ```
            """;
    }

    private static string ExplainRequired(SyntaxToken token, HoverRequest request)
    {
        return """
            **`required`**

            Forces callers to set this property when creating an instance using an object initializer.

            ```csharp
            class Order
            {
                public required string Id { get; init; }
            }

            var order = new Order { Id = "ABC" }; // Id is mandatory
            ```
            """;
    }

    private static string ExplainInit(SyntaxToken token, HoverRequest request)
    {
        return """
            **`init`**

            An `init` accessor allows a property to be set only during object initialization, making it immutable afterward.

            ```csharp
            class Person
            {
                public string Name { get; init; }
            }
            ```
            """;
    }

    private static string ExplainRecord(SyntaxToken token, HoverRequest request)
    {
        return """
            **`record`**

            A reference type with value-based equality. Records are ideal for immutable data models.

            ```csharp
            record Person(string Name, int Age);
            ```

            Records automatically generate `Equals`, `GetHashCode`, `ToString`, and deconstruction.
            """;
    }

    private static string ExplainPartial(SyntaxToken token, HoverRequest request)
    {
        return """
            **`partial`**

            Splits a class, struct, or method definition across multiple files. The compiler combines them.

            ```csharp
            // File1.cs
            partial class MyService { void DoA() { } }

            // File2.cs
            partial class MyService { void DoB() { } }
            ```
            """;
    }

    private static string ExplainAccessModifier(SyntaxToken token, HoverRequest request)
    {
        var keyword = token.Text;
        return keyword switch
        {
            "public" => """
                **`public`**

                Accessible from any code. No access restrictions.
                """,
            "private" => """
                **`private`**

                Accessible only within the containing type. This is the default for class members.
                """,
            "protected" => """
                **`protected`**

                Accessible within the containing type and any derived types.
                """,
            "internal" => """
                **`internal`**

                Accessible within the same assembly, but not from other assemblies.
                """,
            _ => $"**`{keyword}`**\n\nAccess modifier."
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

    private static string ExplainStatic(SyntaxToken token, HoverRequest request)
    {
        return token.Parent switch
        {
            ClassDeclarationSyntax => """
          **`static` class**

          A `static` class cannot be instantiated and can only contain static members. Use it for utility/helper methods.

          ```csharp
          static class MathHelper
          {
              public static int Square(int x) => x * x;
          }
          ```
          """,
            _ => """
          **`static`**

          A `static` member belongs to the type itself, not to any instance. You access it via the type name.

          ```csharp
          class Counter
          {
              public static int Count { get; set; }
          }
          // Counter.Count = 5;
          ```
          """
        };
    }

    private static string ExplainReadonly(SyntaxToken token, HoverRequest request)
    {
        return token.Parent switch
        {
            StructDeclarationSyntax => """
          **`readonly` struct**

          A `readonly` struct guarantees that all members are immutable. The compiler enforces that no instance member modifies state.

          ```csharp
          readonly struct Point(double X, double Y);
          ```
          """,
            _ => """
          **`readonly`**

          A `readonly` field can only be assigned at declaration or in a constructor. It prevents accidental mutation after initialization.

          ```csharp
          class Config
          {
              readonly string _name;
              public Config(string name) => _name = name;
          }
          ```

          Unlike `const`, `readonly` fields are evaluated at runtime and can hold reference types.
          """
        };
    }

    private static string ExplainConst(SyntaxToken token, HoverRequest request)
    {
        return """
        **`const`**

        A compile-time constant. The value must be known at compile time and is inlined wherever it's used.

        ```csharp
        const double Pi = 3.14159265;
        ```

        Unlike `readonly`, `const` is implicitly `static` and only supports primitive types and strings.
        """;
    }

    private static string ExplainClass(SyntaxToken token, HoverRequest request)
    {
        return """
        **`class`**

        Defines a reference type. Instances are allocated on the heap and variables hold references (pointers) to them.

        ```csharp
        class Person
        {
            public string Name { get; set; }
        }
        ```

        Classes support inheritance, interfaces, and polymorphism.
        """;
    }

    private static string ExplainStruct(SyntaxToken token, HoverRequest request)
    {
        return """
        **`struct`**

        Defines a value type. Instances are typically allocated on the stack and copied on assignment.

        ```csharp
        struct Point
        {
            public double X;
            public double Y;
        }
        ```

        Use structs for small, immutable data. They cannot inherit from other types (but can implement interfaces).
        """;
    }

    private static string ExplainInterface(SyntaxToken token, HoverRequest request)
    {
        return """
        **`interface`**

        Defines a contract: a set of members that implementing types must provide. Interfaces support multiple inheritance.

        ```csharp
        interface IShape
        {
            double Area();
        }

        class Circle : IShape
        {
            public double Radius { get; init; }
            public double Area() => Math.PI * Radius * Radius;
        }
        ```
        """;
    }

    private static string ExplainEnum(SyntaxToken token, HoverRequest request)
    {
        return """
        **`enum`**

        Defines a set of named integer constants. Enums make code more readable and type-safe.

        ```csharp
        enum Direction { North, South, East, West }

        Direction d = Direction.North;
        ```
        """;
    }

    private static string ExplainNamespace(SyntaxToken token, HoverRequest request)
    {
        return """
        **`namespace`**

        Organizes code into logical groups and prevents naming conflicts between types.

        ```csharp
        namespace MyApp.Models;

        class User { }
        ```

        File-scoped namespaces (with `;`) apply to the entire file and reduce nesting.
        """;
    }

    private static string ExplainUsing(SyntaxToken token, HoverRequest request)
    {
        if (token.Parent is UsingStatementSyntax)
        {
            return """
          **`using` statement**

          Ensures an `IDisposable` resource is disposed when the block exits, even if an exception occurs.

          ```csharp
          using var stream = File.OpenRead("data.txt");
          // stream is disposed at end of scope
          ```
          """;
        }

        return """
        **`using` directive**

        Imports a namespace so its types can be used without full qualification.

        ```csharp
        using System.Collections.Generic;

        List<int> numbers = new();
        ```
        """;
    }

    private static string ExplainYield(SyntaxToken token, HoverRequest request)
    {
        if (token.Parent is YieldStatementSyntax yieldStatement &&
            yieldStatement.ReturnOrBreakKeyword.IsKind(SyntaxKind.BreakKeyword))
        {
            return """
          **`yield break`**

          Ends an iterator method. No more elements will be produced.

          ```csharp
          IEnumerable<int> FirstFew(int[] items)
          {
              for (int i = 0; i < 3; i++)
              {
                  if (i >= items.Length) yield break;
                  yield return items[i];
              }
          }
          ```
          """;
        }

        return """
        **`yield return`**

        Produces the next element in an iterator. The method's state is preserved between calls.

        ```csharp
        IEnumerable<int> OneTwoThree()
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }
        ```

        The method only executes up to each `yield return` when the consumer requests the next item (lazy evaluation).
        """;
    }

    private static string ExplainIs(SyntaxToken token, HoverRequest request)
    {
        return """
        **`is`**

        Tests whether an expression matches a type or pattern. Returns `true` or `false`.

        ```csharp
        if (obj is string s)
        {
            Console.WriteLine(s.Length);
        }

        if (value is > 0 and < 100)
        {
            // pattern matching
        }
        ```
        """;
    }

    private static string ExplainAs(SyntaxToken token, HoverRequest request)
    {
        return """
        **`as`**

        Attempts a safe type cast. Returns `null` if the cast fails (instead of throwing an exception).

        ```csharp
        var animal = obj as Dog;
        if (animal != null)
        {
            animal.Bark();
        }
        ```

        Prefer `is` with pattern matching in modern C#: `if (obj is Dog dog)`.
        """;
    }

    private static string ExplainNew(SyntaxToken token, HoverRequest request)
    {
        if (token.Parent is ObjectCreationExpressionSyntax or
            ImplicitObjectCreationExpressionSyntax)
        {
            return """
          **`new` (object creation)**

          Creates a new instance of a type.

          ```csharp
          var list = new List<int>();
          List<int> list2 = new(); // target-typed new
          ```
          """;
        }

        return """
        **`new`**

        Depending on context: creates an instance, hides an inherited member, or constrains a generic type parameter.

        ```csharp
        // Object creation
        var obj = new MyClass();

        // Member hiding
        new void Method() { }

        // Generic constraint
        where T : new()
        ```
        """;
    }

    private static string ExplainReturn(SyntaxToken token, HoverRequest request)
    {
        return """
        **`return`**

        Exits the current method and optionally passes a value back to the caller.

        ```csharp
        int Add(int a, int b) => a + b;

        // or
        int Add(int a, int b)
        {
            return a + b;
        }
        ```
        """;
    }

    private static string ExplainVolatile(SyntaxToken token, HoverRequest request)
    {
        return """
        **`volatile`**

        Indicates that a field may be modified by multiple threads. The compiler and runtime avoid certain optimizations that assume single-threaded access.

        ```csharp
        volatile bool _running = true;
        ```

        Use sparingly — prefer `lock` or `Interlocked` for most concurrency scenarios.
        """;
    }

    private static string ExplainExtern(SyntaxToken token, HoverRequest request)
    {
        return """
        **`extern`**

        Declares a method that is implemented externally, typically in a native DLL via P/Invoke.

        ```csharp
        [DllImport("user32.dll")]
        static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
        ```
        """;
    }

    private static string ExplainUnsafe(SyntaxToken token, HoverRequest request)
    {
        return """
        **`unsafe`**

        Enables pointer operations and bypasses runtime safety checks. Required for direct memory manipulation.

        ```csharp
        unsafe
        {
            int value = 42;
            int* ptr = &value;
        }
        ```

        Requires the `AllowUnsafeBlocks` project setting.
        """;
    }

    private static string ExplainWhere(SyntaxToken token, HoverRequest request)
    {
        if (token.Parent is TypeParameterConstraintClauseSyntax)
        {
            return """
          **`where` (generic constraint)**

          Restricts which types can be used as a type argument for a generic parameter.

          ```csharp
          class Repository<T> where T : class, IEntity, new()
          {
          }
          ```

          Common constraints: `class`, `struct`, `new()`, `notnull`, interface/base class.
          """;
        }

        return """
        **`where`**

        In LINQ, filters a sequence. In generics, constrains a type parameter.

        ```csharp
        // LINQ
        var adults = people.Where(p => p.Age >= 18);

        // Generic constraint
        void Save<T>(T item) where T : ISerializable { }
        ```
        """;
    }

    private static string ExplainWhen(SyntaxToken token, HoverRequest request)
    {
        return """
        **`when`**

        Adds a condition to a `catch` clause or a `switch` case pattern.

        ```csharp
        catch (Exception ex) when (ex.Message.Contains("timeout"))
        {
            // only catches timeout exceptions
        }

        switch (shape)
        {
            case Circle c when c.Radius > 10:
                break;
        }
        ```
        """;
    }

    private static string ExplainIn(SyntaxToken token, HoverRequest request)
    {
        if (token.Parent is TypeParameterSyntax or TypeParameterConstraintClauseSyntax)
        {
            return """
          **`in` (contravariant)**

          Marks a generic type parameter as contravariant — it can only be used as input (method parameters).

          ```csharp
          interface IComparer<in T>
          {
              int Compare(T x, T y);
          }
          ```
          """;
        }

        if (token.Parent is ForEachStatementSyntax)
        {
            return """
          **`in` (foreach)**

          Iterates over each element in a collection.

          ```csharp
          foreach (var item in collection)
          {
              Console.WriteLine(item);
          }
          ```
          """;
        }

        return """
        **`in` (parameter modifier)**

        Passes an argument by reference but prevents the method from modifying it. Useful for large structs to avoid copying.

        ```csharp
        void Print(in Point p) => Console.WriteLine($"{p.X}, {p.Y}");
        ```
        """;
    }

    private static string ExplainOut(SyntaxToken token, HoverRequest request)
    {
        if (token.Parent is TypeParameterSyntax)
        {
            return """
          **`out` (covariant)**

          Marks a generic type parameter as covariant — it can only be used as output (return types).

          ```csharp
          interface IFactory<out T>
          {
              T Create();
          }
          ```
          """;
        }

        return """
        **`out` (parameter modifier)**

        Passes an argument by reference. The method must assign a value before returning.

        ```csharp
        if (int.TryParse(input, out int result))
        {
            Console.WriteLine(result);
        }
        ```
        """;
    }

    private static string ExplainRef(SyntaxToken token, HoverRequest request)
    {
        return """
        **`ref`**

        Passes an argument by reference. Changes inside the method affect the original variable.

        ```csharp
        void Increment(ref int x) => x++;

        int val = 5;
        Increment(ref val); // val is now 6
        ```
        """;
    }

    private static string ExplainParams(SyntaxToken token, HoverRequest request)
    {
        return """
        **`params`**

        Allows a method to accept a variable number of arguments as an array.

        ```csharp
        void Log(params string[] messages)
        {
            foreach (var msg in messages)
                Console.WriteLine(msg);
        }

        Log("hello", "world");
        ```
        """;
    }

    private static string ExplainThis(SyntaxToken token, HoverRequest request)
    {
        if (token.Parent is ParameterSyntax)
        {
            return """
          **`this` (extension method)**

          The `this` modifier on the first parameter declares an extension method.

          ```csharp
          static class StringExtensions
          {
              public static bool IsEmpty(this string s) => s.Length == 0;
          }

          // Usage: "hello".IsEmpty()
          ```
          """;
        }

        return """
        **`this`**

        Refers to the current instance of the class. Used to access members or pass the instance itself.

        ```csharp
        class Person
        {
            string name;
            public Person(string name) => this.name = name;
        }
        ```
        """;
    }

    private static string ExplainBase(SyntaxToken token, HoverRequest request)
    {
        return """
        **`base`**

        Accesses members of the base class. Commonly used to call the base constructor or a base method you're overriding.

        ```csharp
        class Dog : Animal
        {
            public Dog(string name) : base(name) { }

            public override void Speak()
            {
                base.Speak(); // call base implementation
                Console.WriteLine("Woof");
            }
        }
        ```
        """;
    }

    private static string ExplainLock(SyntaxToken token, HoverRequest request)
    {
        return """
        **`lock`**

        Acquires a mutual-exclusion lock, ensuring only one thread executes the block at a time.

        ```csharp
        lock (_syncObj)
        {
            _count++;
        }
        ```

        Always lock on a dedicated private object, never on `this` or a `Type`.
        """;
    }

    private static string ExplainDelegate(SyntaxToken token, HoverRequest request)
    {
        return """
        **`delegate`**

        Defines a type that represents a method signature. Delegates enable callbacks, events, and LINQ.

        ```csharp
        delegate int MathOp(int a, int b);

        MathOp add = (a, b) => a + b;
        ```

        In modern C#, prefer `Func<>` and `Action<>` for most cases.
        """;
    }

    private static string ExplainEvent(SyntaxToken token, HoverRequest request)
    {
        return """
        **`event`**

        Declares a notification mechanism. Only the containing type can raise it; external code can subscribe or unsubscribe.

        ```csharp
        class Button
        {
            public event EventHandler? Clicked;

            public void Press() => Clicked?.Invoke(this, EventArgs.Empty);
        }

        button.Clicked += (s, e) => Console.WriteLine("Pressed!");
        ```
        """;
    }
}
