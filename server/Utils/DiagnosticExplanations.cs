using Microsoft.CodeAnalysis;
using Server.Models;

namespace Server.Utils;

/// <summary>
/// Curated plain-English explanations for high-frequency C# diagnostic IDs.
/// Each entry returns educational markdown: what it means, why it happens,
/// common fixes, and links to related language concepts.
/// </summary>
public static class DiagnosticExplanations
{
    private static readonly Dictionary<string, Func<Diagnostic, DiagnosticExplanationRequest, string>> Explanations = new()
    {
        ["CS0103"] = ExplainCS0103,
        ["CS1002"] = ExplainCS1002,
        ["CS0246"] = ExplainCS0246,
        ["CS0029"] = ExplainCS0029,
        ["CS0019"] = ExplainCS0019,
        ["CS0117"] = ExplainCS0117,
        ["CS1061"] = ExplainCS1061,
        ["CS0428"] = ExplainCS0428,
        ["CS0234"] = ExplainCS0234,
        ["CS0161"] = ExplainCS0161,
        ["CS0165"] = ExplainCS0165,
        ["CS0266"] = ExplainCS0266,
        ["CS8600"] = ExplainCS8600,
        ["CS8602"] = ExplainCS8602,
        ["CS0120"] = ExplainCS0120,
        ["CS1503"] = ExplainCS1503,
        ["CS0106"] = ExplainCS0106,
        ["CS0535"] = ExplainCS0535,
        ["CS0534"] = ExplainCS0534,
        ["CS0115"] = ExplainCS0115,
        ["CS0227"] = ExplainCS0227,
        ["CS0168"] = ExplainCS0168,
        ["CS0219"] = ExplainCS0219,
        ["CS0162"] = ExplainCS0162,
        ["CS0128"] = ExplainCS0128,
        ["CS0122"] = ExplainCS0122,
        ["CS1513"] = ExplainCS1513,
        ["CS1525"] = ExplainCS1525,
        ["CS8618"] = ExplainCS8618,
        ["CS8604"] = ExplainCS8604,
        ["CS0305"] = ExplainCS0305,
        ["CS0118"] = ExplainCS0118,
        ["CS0200"] = ExplainCS0200,
        ["CS1729"] = ExplainCS1729,
        ["CS0618"] = ExplainCS0618,
        ["CS0131"] = ExplainCS0131,
        ["CS0111"] = ExplainCS0111,
        ["CS0236"] = ExplainCS0236,
        ["CS0012"] = ExplainCS0012,
    };

    /// <summary>
    /// Returns a markdown explanation for the given diagnostic, or a generic
    /// fallback if the diagnostic ID is not in the curated set.
    /// </summary>
    public static string GetExplanation(Diagnostic diagnostic, DiagnosticExplanationRequest request)
    {
        if (Explanations.TryGetValue(diagnostic.Id, out var explain))
        {
            return explain(diagnostic, request);
        }

        return GetFallbackExplanation(diagnostic);
    }

    // ── CS0103: The name 'X' does not exist in the current context ──

    private static string ExplainCS0103(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS0103 — Name not found**

            > {d.GetMessage()}

            **What it means:** You're using a name (variable, method, or type) that the compiler can't find in the current scope.

            **Why it happens:**
            - The variable hasn't been declared yet, or is declared in a different scope (e.g., inside another `if` or `for` block).
            - A method or type name is misspelled.
            - A required `using` directive is missing.

            **Common fixes:**
            1. Check spelling — C# is case-sensitive (`myVar` ≠ `MyVar`).
            2. Make sure the variable is declared before it's used.
            3. Add the appropriate `using` directive for the namespace.
            4. Check that the referenced assembly or project is included.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                // ❌ Error: 'count' does not exist
                Console.WriteLine(count);

                // ✅ Fix: declare first
                int count = 10;
                Console.WriteLine(count);
                ```
                """;
        }

        return md + RelatedConcept("using", "The `using` directive imports namespaces so their types are available without full qualification.");
    }

    // ── CS1002: ; expected ──

    private static string ExplainCS1002(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS1002 — Semicolon expected**

            > {d.GetMessage()}

            **What it means:** The compiler reached a point where a semicolon (`;`) is required to end a statement, but didn't find one.

            **Why it happens:**
            - A statement is missing its terminating `;`.
            - A closing parenthesis or bracket is misplaced, causing the parser to see the next line as a continuation.

            **Common fixes:**
            1. Add the missing `;` at the end of the statement.
            2. Check for unmatched parentheses, brackets, or braces on the line above.

            **Example:**
            ```csharp
            // ❌ Error
            int x = 5

            // ✅ Fix
            int x = 5;
            ```
            """;
    }

    // ── CS0246: The type or namespace name 'X' could not be found ──

    private static string ExplainCS0246(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS0246 — Type or namespace not found**

            > {d.GetMessage()}

            **What it means:** The compiler can't find the type or namespace you're referring to.

            **Why it happens:**
            - A `using` directive for the namespace is missing.
            - The type name is misspelled.
            - The assembly or NuGet package containing the type isn't referenced.
            - The type is defined in a different project that isn't added as a project reference.

            **Common fixes:**
            1. Add a `using` directive: `using Namespace.Name;`
            2. Install the NuGet package: `dotnet add package PackageName`
            3. Add a project reference: `dotnet add reference ../OtherProject`
            4. Check spelling and casing.
            """;

        return md + RelatedConcept("using", "The `using` directive imports namespaces so their types are available without full qualification.");
    }

    // ── CS0029: Cannot implicitly convert type 'X' to 'Y' ──

    private static string ExplainCS0029(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS0029 — Cannot implicitly convert type**

            > {d.GetMessage()}

            **What it means:** You're assigning or returning a value of one type where a different, incompatible type is expected. C# doesn't allow this conversion implicitly.

            **Why it happens:**
            - Assigning a `string` to an `int` variable (or similar type mismatch).
            - Returning the wrong type from a method.
            - Passing an argument of the wrong type to a method.

            **Common fixes:**
            1. Use an explicit cast: `(TargetType)value`
            2. Use a conversion method: `int.Parse(str)`, `Convert.ToInt32(value)`
            3. Change the variable or return type to match.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                // ❌ Error: cannot convert string to int
                int count = "hello";

                // ✅ Fix: use correct types
                string name = "hello";
                int count = int.Parse("42");
                ```
                """;
        }

        return md;
    }

    // ── CS0019: Operator cannot be applied to operands of type 'X' and 'Y' ──

    private static string ExplainCS0019(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS0019 — Operator cannot be applied**

            > {d.GetMessage()}

            **What it means:** You're using an operator (like `+`, `-`, `==`, `>`) between two types that don't support that operation.

            **Why it happens:**
            - Comparing or combining incompatible types (e.g., `string > int`).
            - Using arithmetic operators on non-numeric types.
            - The type doesn't define that operator.

            **Common fixes:**
            1. Convert one operand to a compatible type.
            2. Use the correct comparison method (e.g., `string.Compare()` instead of `>`).
            3. Implement the operator on your custom type if needed.
            """;
    }

    // ── CS0117: 'X' does not contain a definition for 'Y' ──

    private static string ExplainCS0117(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS0117 — Type does not contain a definition for member**

            > {d.GetMessage()}

            **What it means:** You're trying to access a member (method, property, or field) that doesn't exist on the specified type.

            **Why it happens:**
            - The member name is misspelled.
            - You're calling a method on the wrong type.
            - You're using `base.Member` or `Type.Member` for a member that the referenced type doesn't define.

            **Common fixes:**
            1. Check the spelling and casing of the member name.
            2. Verify you're using the correct type.
                3. If the member is an extension method, add the `using` directive for its namespace or call the correct instance/type member instead.
            """ + RelatedConcept("static", "A `static` member belongs to the type itself, not to any instance. Access it via the type name: `ClassName.Member`.");
    }

    // ── CS1061: 'X' does not contain a definition for 'Y' (instance + extension methods) ──

    private static string ExplainCS1061(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS1061 — Type does not contain a definition (including extension methods)**

            > {d.GetMessage()}

            **What it means:** You're calling a method or accessing a property that doesn't exist on this type, and no applicable extension method was found either.

            **Why it happens:**
            - The member name is misspelled.
            - You need a `using` directive to bring an extension method into scope.
            - The type you think you're working with is actually different (check `var` inference).

            **Common fixes:**
            1. Check the spelling — C# is case-sensitive.
            2. Add the `using` directive for the extension method's namespace (e.g., `using System.Linq;` for `.Select()`, `.Where()`, etc.).
            3. Hover over the variable to check its actual type.
            """ + RelatedConcept("using", "The `using` directive imports namespaces. Extension methods like LINQ's `.Where()` require `using System.Linq;`.");
    }

    // ── CS0428: Cannot convert method group to non-delegate type ──

    private static string ExplainCS0428(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS0428 — Cannot convert method group to non-delegate type**

            > {d.GetMessage()}

            **What it means:** You're referencing a method name without calling it (missing parentheses `()`), and the compiler can't convert the method reference to the expected type.

            **Why it happens:**
            - You wrote `obj.Method` when you meant `obj.Method()`.
            - You're trying to use a method as a value but it's not a delegate context.

            **Common fixes:**
            1. Add parentheses to call the method: `obj.Method()` instead of `obj.Method`.
            2. If you intend to pass the method as a callback, assign it to a delegate or `Func<>`/`Action<>`.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                // ❌ Error: missing parentheses
                string result = obj.ToString;

                // ✅ Fix: call the method
                string result = obj.ToString();
                ```
                """;
        }

        return md + RelatedConcept("delegate", "A `delegate` is a type that represents a method signature, enabling callbacks and event patterns.");
    }

    // ── CS0234: The type or namespace name 'X' does not exist in the namespace 'Y' ──

    private static string ExplainCS0234(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS0234 — Type or namespace does not exist in namespace**

            > {d.GetMessage()}

            **What it means:** You're trying to use a type or sub-namespace that doesn't exist within the specified namespace.

            **Why it happens:**
            - The type moved to a different namespace in a newer API version.
            - The NuGet package or project reference is missing.
            - The namespace path is misspelled.

            **Common fixes:**
            1. Verify the correct namespace in the library's documentation.
            2. Add or update the NuGet package: `dotnet add package PackageName`
            3. Check for typos in the namespace path.
            """ + RelatedConcept("namespace", "Namespaces organize code into logical groups. Use `using` directives to import them.");
    }

    // ── CS0161: Not all code paths return a value ──

    private static string ExplainCS0161(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS0161 — Not all code paths return a value**

            > {d.GetMessage()}

            **What it means:** A method with a non-void return type has at least one execution path that doesn't return a value. The compiler must guarantee every path returns something.

            **Why it happens:**
            - An `if`/`else` chain is missing a final `else` with a `return`.
            - A `switch` statement doesn't cover all cases.
            - An early `return` exists in one branch but not all.

            **Common fixes:**
            1. Add a `return` statement in every branch.
            2. Add a default `else` or `default:` case.
            3. Add a final `return` at the end of the method as a catch-all.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                // ❌ Error: no return when x <= 0
                int Abs(int x)
                {
                    if (x > 0) return x;
                }

                // ✅ Fix: cover all paths
                int Abs(int x)
                {
                    if (x > 0) return x;
                    return -x;
                }
                ```
                """;
        }

        return md + RelatedConcept("return", "The `return` keyword exits the current method and passes a value back to the caller.");
    }

    // ── CS0165: Use of unassigned local variable 'X' ──

    private static string ExplainCS0165(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS0165 — Use of unassigned local variable**

            > {d.GetMessage()}

            **What it means:** You're reading a local variable that hasn't been assigned a value yet. C# requires definite assignment — every local must have a value before it's read.

            **Why it happens:**
            - The variable is declared but only assigned inside a conditional branch.
            - The variable is declared without an initializer and used before any assignment.

            **Common fixes:**
            1. Assign a default value at declaration: `int count = 0;`
            2. Ensure every code path assigns the variable before it's read.
            3. Use `out` parameter pattern: `if (dict.TryGetValue(key, out var value))`.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                // ❌ Error: 'result' might not be assigned
                int result;
                if (condition) result = 42;
                Console.WriteLine(result);

                // ✅ Fix: assign a default
                int result = 0;
                if (condition) result = 42;
                Console.WriteLine(result);
                ```
                """;
        }

        return md;
    }

    // ── CS0266: Cannot implicitly convert type (explicit conversion exists) ──

    private static string ExplainCS0266(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS0266 — Cannot implicitly convert type (explicit conversion exists)**

            > {d.GetMessage()}

            **What it means:** The types are related and a conversion exists, but it could lose data, so C# requires you to write an explicit cast to confirm you accept the risk.

            **Why it happens:**
            - Assigning a `double` to an `int` (loses decimal part).
            - Assigning a base type to a derived type variable (downcast).
            - Assigning a `long` to an `int` (could overflow).

            **Common fixes:**
            1. Add an explicit cast: `int x = (int)doubleValue;`
            2. Use a safe conversion method: `Convert.ToInt32(value)`
            3. Change the target variable type to match.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                // ❌ Error: cannot implicitly convert double to int
                double pi = 3.14;
                int rounded = pi;

                // ✅ Fix: explicit cast
                int rounded = (int)pi;
                ```
                """;
        }

        return md;
    }

    // ── CS8600: Converting null literal or possible null value to non-nullable type ──

    private static string ExplainCS8600(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS8600 — Converting null to non-nullable type**

            > {d.GetMessage()}

            **What it means:** You're assigning `null` (or a possibly-null value) to a variable whose type doesn't allow null. This is a **nullable reference type** warning — it helps prevent `NullReferenceException` at runtime.

            **Why it happens:**
            - Assigning `null` to a `string` instead of `string?`.
            - A method returns a nullable type but the receiving variable is non-nullable.

            **Common fixes:**
            1. Make the variable nullable: `string? name = null;`
            2. Provide a non-null default: `string name = value ?? "default";`
            3. Add a null check before the assignment.
            """ + RelatedConcept("nullable",
                "C# nullable reference types (`string?`) help the compiler warn you about potential `null` dereferences. Enable with `#nullable enable` or in your `.csproj`.");
    }

    // ── CS8602: Dereference of a possibly null reference ──

    private static string ExplainCS8602(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS8602 — Dereference of a possibly null reference**

            > {d.GetMessage()}

            **What it means:** You're accessing a member (`.Property`, `.Method()`) on a value that could be `null`. This could throw `NullReferenceException` at runtime.

            **Why it happens:**
            - A variable is typed as nullable (`string?`) and you're using it without a null check.
            - A method might return `null` and you're chaining calls on the result.

            **Common fixes:**
            1. Add a null check: `if (value != null) value.DoSomething();`
            2. Use null-conditional operator: `value?.DoSomething();`
            3. Use null-coalescing: `var safe = value ?? fallback;`
            4. Use the `!` operator if you're certain it's not null: `value!.DoSomething();` (use with caution).
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                // ❌ Warning: 'name' may be null
                string? name = GetName();
                int len = name.Length;

                // ✅ Fix: null-conditional
                int? len = name?.Length;

                // ✅ Fix: null check
                if (name is not null)
                {
                    int len = name.Length;
                }
                ```
                """;
        }

        return md + RelatedConcept("nullable",
            "Nullable reference types (`string?`) make null safety a first-class feature. The compiler tracks nullability and warns when you might dereference `null`.");
    }

    // ── CS0120: An object reference is required for the non-static member ──

    private static string ExplainCS0120(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS0120 — Object reference required for non-static member**

            > {d.GetMessage()}

            **What it means:** You're trying to access an instance member (field, property, or method) without an instance — typically from a `static` method or directly on the class name.

            **Why it happens:**
            - Calling an instance method from a `static` method like `Main()`.
            - Using `ClassName.InstanceMember` instead of creating an instance first.

            **Common fixes:**
            1. Create an instance: `var obj = new MyClass(); obj.Method();`
            2. Make the member `static` if it doesn't need instance state.
            3. If in `Main()`, create an instance of the containing class.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                class MyApp
                {
                    string name = "World";

                    // ❌ Error: can't access 'name' from static context
                    static void Main()
                    {
                        Console.WriteLine(name);
                    }

                    // ✅ Fix: create an instance
                    static void Main()
                    {
                        var app = new MyApp();
                        Console.WriteLine(app.name);
                    }
                }
                ```
                """;
        }

        return md + RelatedConcept("static",
            "A `static` member belongs to the type itself, not to any instance. Instance members require an object created with `new`.");
    }

    // ── CS1503: Argument type mismatch ──

    private static string ExplainCS1503(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS1503 — Argument type mismatch**

            > {d.GetMessage()}

            **What it means:** You're passing an argument to a method, but its type doesn't match the parameter type, and no implicit conversion exists.

            **Why it happens:**
            - Passing a `string` where an `int` is expected (or similar).
            - Passing arguments in the wrong order.
            - Using a variable of the wrong type.

            **Common fixes:**
            1. Convert the argument: `int.Parse(str)`, `value.ToString()`.
            2. Check the method signature to see the expected parameter types.
            3. Check argument order — the types might be correct but swapped.
            """;
    }

    // ── CS0106: The modifier 'X' is not valid for this item ──

    private static string ExplainCS0106(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS0106 — Modifier not valid for this item**

            > {d.GetMessage()}

            **What it means:** You've applied a modifier (`static`, `abstract`, `override`, etc.) to a declaration where it's not allowed.

            **Why it happens:**
            - Marking a local variable as `public` or `static`.
            - Adding `public` or `abstract` to an explicit interface implementation.
            - Using a modifier that is valid on one declaration kind but not on this one, such as `volatile` on a method.

            **Common fixes:**
            1. Remove the invalid modifier.
            2. Apply the modifier to the correct kind of declaration.
            3. For explicit interface implementations, remove accessibility modifiers like `public`.
            """ + RelatedConcept("abstract",
                "The `abstract` modifier marks a class or member as incomplete — derived classes must provide the implementation via `override`.");
    }

    // ── CS0535: Class does not implement interface member ──

    private static string ExplainCS0535(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS0535 — Does not implement interface member**

            > {d.GetMessage()}

            **What it means:** Your class declares that it implements an interface, but it's missing one or more required members from that interface.

            **Why it happens:**
            - You added the interface to the class declaration but forgot to implement all its members.
            - The method signature doesn't exactly match (wrong return type, parameter types, or name).
            - The member is implemented but not `public`.

            **Common fixes:**
            1. Implement the missing member with the exact signature from the interface.
            2. Check that the method is `public` (interface members are public by default).
            3. Use your IDE's "Implement interface" quick action to auto-generate stubs.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                interface IShape { double Area(); }

                // ❌ Error: missing Area()
                class Circle : IShape { }

                // ✅ Fix: implement the member
                class Circle : IShape
                {
                    public double Radius { get; set; }
                    public double Area() => Math.PI * Radius * Radius;
                }
                ```
                """;
        }

        return md + RelatedConcept("interface",
            "An `interface` defines a contract — a set of members that implementing types must provide. Interfaces support multiple inheritance.");
    }

    // ── CS0534: Class does not implement inherited abstract member ──

    private static string ExplainCS0534(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS0534 — Does not implement inherited abstract member**

            > {d.GetMessage()}

            **What it means:** Your class inherits from an abstract base class but doesn't override all of its abstract members. A non-abstract class must provide implementations for every abstract member it inherits.

            **Why it happens:**
            - You extended an abstract class but forgot to override one or more abstract methods/properties.
            - The override signature doesn't match exactly.

            **Common fixes:**
            1. Add `override` implementations for each abstract member.
            2. If the class is intentionally incomplete, mark it `abstract` too.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                abstract class Shape
                {
                    public abstract double Area();
                }

                // ❌ Error: 'Circle' does not implement 'Shape.Area()'
                class Circle : Shape { }

                // ✅ Fix: override the abstract member
                class Circle : Shape
                {
                    public double Radius { get; set; }
                    public override double Area() => Math.PI * Radius * Radius;
                }
                ```
                """;
        }

        return md + RelatedConcepts(
            ("abstract", "An `abstract` class cannot be instantiated and may contain abstract members that derived classes must implement."),
            ("override", "The `override` keyword provides a new implementation of a `virtual` or `abstract` member from a base class."));
    }

    // ── CS0115: No suitable method found to override ──

    private static string ExplainCS0115(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS0115 — No suitable method found to override**

            > {d.GetMessage()}

            **What it means:** You marked a method with `override`, but the base class doesn't have a matching `virtual` or `abstract` method with the same signature.

            **Why it happens:**
            - The base class method name or parameter types don't match.
            - The base class method isn't marked `virtual` or `abstract`.
            - The class doesn't inherit from the type you think it does.

            **Common fixes:**
            1. Check the base class for the exact method signature (name, return type, parameters).
            2. Mark the base method as `virtual` if you intend to allow overriding.
            3. Remove `override` if this is a new method, not an override.
            """ + RelatedConcepts(
                ("virtual", "The `virtual` keyword allows a method to be overridden in derived classes."),
                ("override", "The `override` keyword provides a new implementation of an inherited `virtual` or `abstract` member."));
    }

    // ── CS0227: Unsafe code requires /unsafe ──

    private static string ExplainCS0227(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS0227 — Unsafe code requires the `unsafe` option**

            > {d.GetMessage()}

            **What it means:** Your code uses `unsafe` features (pointers, `fixed`, `stackalloc`, etc.) but the project isn't configured to allow them.

            **Why it happens:**
            - A method or block is marked `unsafe`, or you're using pointer types like `int*`.
            - The compiler requires explicit opt-in because unsafe code bypasses runtime safety checks.

            **Common fixes:**
            1. Add `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` to the `<PropertyGroup>` in your `.csproj` file.
            2. Or remove the `unsafe` keyword if pointer operations aren't needed.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example `.csproj` fix:**
                ```xml
                <PropertyGroup>
                    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
                </PropertyGroup>
                ```
                """;
        }

        return md + RelatedConcept("unsafe",
            "The `unsafe` keyword enables pointer operations and bypasses runtime safety checks. It requires the `AllowUnsafeBlocks` project setting.");
    }

    // ── CS0168: Variable declared but never used ──

    private static string ExplainCS0168(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS0168 — Variable declared but never used**

            > {d.GetMessage()}

            **What it means:** You declared a variable but never read its value anywhere. This is a warning — the code compiles, but the variable is dead code.

            **Why it happens:**
            - You declared a variable for future use but haven't used it yet.
            - The variable was part of removed logic but the declaration was left behind.
            - A `catch` block declares an exception variable that's never inspected.

            **Common fixes:**
            1. Use the variable, or remove the declaration.
                2. In `catch` blocks, omit the variable name: `catch (Exception)` if you don't need the exception object.
            3. Prefix with `_` to signal intentional non-use: `var _unused = ...;`
            """;
    }

    // ── CS0219: Variable assigned but never used ──

    private static string ExplainCS0219(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS0219 — Variable assigned but its value is never used**

            > {d.GetMessage()}

            **What it means:** You assigned a value to a variable, but never read it. The assignment has no effect on the program's behavior.

            **Why it happens:**
            - You stored a result you intended to use later but forgot.
            - Old code was partially removed, leaving a dangling assignment.
            - A return value is captured but never checked.

            **Common fixes:**
            1. Use the variable in subsequent code.
            2. Remove the variable and its assignment.
            3. If the call has side effects, use a discard: `_ = SomeMethod();`
            """;
    }

    // ── CS0162: Unreachable code detected ──

    private static string ExplainCS0162(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS0162 — Unreachable code detected**

            > {d.GetMessage()}

            **What it means:** The compiler determined that some code can never execute because a previous statement always exits the method, breaks, or continues.

            **Why it happens:**
            - Code appears after an unconditional `return`, `throw`, `break`, or `continue`.
            - A condition is always true or always false (e.g., `if (true)`).

            **Common fixes:**
            1. Remove the unreachable code.
            2. Check your control flow — you may have an accidental early `return`.
            3. If the dead code is intentional (debugging), suppress the warning temporarily.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                int M()
                {
                    return 42;
                    Console.WriteLine("never runs"); // ⚠ unreachable
                }
                ```
                """;
        }

        return md + RelatedConcept("return", "The `return` keyword exits the current method immediately. Code after an unconditional `return` is unreachable.");
    }

    // ── CS0128: Local variable already defined in scope ──

    private static string ExplainCS0128(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS0128 — A local variable is already defined in this scope**

            > {d.GetMessage()}

            **What it means:** You declared two local variables with the same name in the same scope. C# doesn't allow duplicate names within the same block.

            **Why it happens:**
            - Copy-pasting code without renaming variables.
            - Declaring a local variable with the same name as another local already declared in that block.
            - Re-declaring a variable when you meant to assign a new value to the existing one.

            **Common fixes:**
            1. Rename one of the variables.
            2. Reuse the existing variable instead of re-declaring it.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                // ❌ Error: 'x' already defined
                int x = 1;
                int x = 2;

                // ✅ Fix: rename or reuse
                int x = 1;
                int y = 2;
                ```
                """;
        }

        return md;
    }

    // ── CS0122: Inaccessible due to protection level ──

    private static string ExplainCS0122(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS0122 — Inaccessible due to its protection level**

            > {d.GetMessage()}

            **What it means:** You're trying to access a member (field, method, property, or type) that is not visible from the current context because of its access modifier.

            **Why it happens:**
            - Accessing a `private` member from outside its class.
            - Accessing a `protected` member from a class that doesn't inherit from the declaring class.
            - Accessing an `internal` member from a different assembly.

            **Common fixes:**
            1. Change the member's access modifier to `public` or `internal`.
            2. Access it through a public method or property instead (encapsulation).
            3. If using `protected`, ensure you're in a derived class.
            """ + RelatedConcepts(
                ("public", "Accessible from any code. No access restrictions."),
                ("private", "Accessible only within the containing type."),
                ("protected", "Accessible within the containing type and derived types."),
                ("internal", "Accessible within the same assembly."));
    }

    // ── CS1513: } expected ──

    private static string ExplainCS1513(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS1513 — Closing brace expected**

            > {d.GetMessage()}

            **What it means:** The compiler expected a closing brace to end a block (class, method, namespace, etc.) but didn't find one.

            **Why it happens:**
            - A closing brace is missing for a class, method, or control structure.
            - Braces are mismatched — an extra opening brace without a matching close.
            - Code was partially deleted or pasted incorrectly.

            **Common fixes:**
            1. Count your opening and closing braces — they must match.
            2. Use your editor's bracket matching (highlight a brace to see its pair).
            3. Check for accidental deletion of closing braces.
            """;
    }

    // ── CS1525: Invalid expression term ──

    private static string ExplainCS1525(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS1525 — Invalid expression term**

            > {d.GetMessage()}

            **What it means:** The compiler found a token where it expected a valid expression (a value, variable, method call, etc.) but the token can't start an expression.

            **Why it happens:**
            - A typo or stray character in the middle of an expression.
            - Missing an operand: `int x = + 5;` or an empty `if` condition.
            - Using a keyword where a value is expected.

            **Common fixes:**
            1. Check for typos or missing operands.
            2. Ensure expressions are complete: every operator has its operands.
            3. Remove stray characters or punctuation.
            """;
    }

    // ── CS8618: Non-nullable field/property must contain non-null when exiting constructor ──

    private static string ExplainCS8618(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS8618 — Non-nullable member must contain a non-null value when exiting constructor**

            > {d.GetMessage()}

            **What it means:** A field or property with a non-nullable reference type (e.g., `string` instead of `string?`) isn't assigned in the constructor. It could be `null` at runtime despite the type saying otherwise.

            **Why it happens:**
            - A `string` or other reference-type property is declared without a default value.
            - The constructor doesn't assign all non-nullable members.

            **Common fixes:**
            1. Initialize the member at declaration with a default value like `= ""`.
            2. Assign it in the constructor.
            3. Make it nullable if null is valid by adding `?` to the type.
            4. Use the `required` keyword to force callers to set it at construction time.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                // ⚠ Warning: 'Name' is not initialized
                class Person
                {
                    public string Name { get; set; }
                }

                // ✅ Fix: provide a default
                class Person
                {
                    public string Name { get; set; } = "";
                }
                ```
                """;
        }

        return md + RelatedConcept("nullable",
            "Nullable reference types help prevent `NullReferenceException`. Non-nullable members must always have a value.");
    }

    // ── CS8604: Possible null reference argument ──

    private static string ExplainCS8604(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS8604 — Possible null reference argument**

            > {d.GetMessage()}

            **What it means:** You're passing a value that might be `null` to a parameter that doesn't accept null. This could cause a `NullReferenceException` if the method doesn't handle null.

            **Why it happens:**
            - A nullable variable (`string?`) is passed to a method expecting `string`.
            - A method return value that could be null is passed directly as an argument.

            **Common fixes:**
            1. Add a null check before the call: `if (value is not null) Method(value);`
            2. Use null-coalescing to provide a fallback: `Method(value ?? "default")`
            3. Make the parameter nullable: change `string param` to `string? param`.
            4. Use `!` if you're certain it's not null: `Method(value!)` (use with caution).
            """ + RelatedConcept("nullable",
                "Nullable reference types track whether a value could be `null`. The compiler warns when a possibly-null value flows into a non-null context.");
    }

    // ── CS0305: Using generic type requires N type arguments ──

    private static string ExplainCS0305(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS0305 — Using generic type requires type arguments**

            > {d.GetMessage()}

            **What it means:** You're using a generic type (like `List`, `Dictionary`, `Task`) without specifying its type parameters in angle brackets.

            **Why it happens:**
            - Writing `List` instead of `List<int>`.
            - Forgetting type arguments on a custom generic class.

            **Common fixes:**
            1. Add the type arguments: `List<string>`, `Dictionary<string, int>`, `Task<bool>`.
            2. Check the type's definition to see how many type parameters it requires.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                // ❌ Error: missing type argument
                List names = new List();

                // ✅ Fix: specify the type
                List<string> names = new List<string>();
                ```
                """;
        }

        return md;
    }

    // ── CS0118: Name is used as a type but is a namespace ──

    private static string ExplainCS0118(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS0118 — Name used incorrectly**

            > {d.GetMessage()}

            **What it means:** You're using a name in a context where a different kind of symbol is expected — for example, using a namespace name where a type is expected, or a type where a variable is expected.

            **Why it happens:**
            - Using a namespace as if it were a type: `System x = ...;`
            - Using a method name where a type is expected.
            - Name collision between a namespace and a type.

            **Common fixes:**
            1. Use the fully qualified type name: `System.IO.Stream` instead of `System.IO`.
            2. Check that you're referencing the correct symbol.
            3. Add an alias if there's a name collision: `using MyType = Namespace.Type;`
            """ + RelatedConcept("namespace", "Namespaces organize code into logical groups. They cannot be used as types directly.");
    }

    // ── CS0200: Property or indexer is read-only ──

    private static string ExplainCS0200(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS0200 — Property or indexer cannot be assigned to — it is read-only**

            > {d.GetMessage()}

            **What it means:** You're trying to assign a value to a property or indexer that is read-only in this context.

            **Why it happens:**
            - The property is defined as `get`-only (computed or read-only).
            - The indexer exposes values for reading but doesn't define a setter.
            - A get-only auto-property is being assigned outside the declaring type's constructor or initializer.

            **Common fixes:**
            1. Add a `set` accessor to the property.
            2. Use `init` if it should only be settable during construction.
            3. Set get-only values in the declaring type's constructor, or pass the value through an existing constructor.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                class Point
                {
                    public int X { get; } // read-only!
                }

                var p = new Point();
                p.X = 5; // ❌ Error

                // ✅ Fix: add set or init
                class Point
                {
                    public int X { get; set; }
                }
                ```
                """;
        }

        return md + RelatedConcept("init", "An `init` accessor allows a property to be set only during object initialization, making it immutable afterward.");
    }

    // ── CS1729: Type does not contain a constructor that takes N arguments ──

    private static string ExplainCS1729(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS1729 — No constructor with that many arguments**

            > {d.GetMessage()}

            **What it means:** You're calling `new` on a type and passing arguments, but the type doesn't have a constructor that accepts that number or those types of parameters.

            **Why it happens:**
            - Passing too many or too few arguments to the constructor.
            - The argument types don't match any constructor overload.
            - The class only has a default (parameterless) constructor.

            **Common fixes:**
            1. Check the class definition for available constructors.
            2. Add a constructor that accepts the arguments you need.
            3. Use object initializer syntax to set properties during construction.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                class Person { }

                // ❌ Error: no constructor takes 1 argument
                var p = new Person("Alice");

                // ✅ Fix: add the constructor
                class Person
                {
                    public string Name { get; }
                    public Person(string name) => Name = name;
                }
                ```
                """;
        }

        return md + RelatedConcept("new", "The `new` keyword creates an instance of a type by calling one of its constructors.");
    }

    // ── CS0618: Member is obsolete ──

    private static string ExplainCS0618(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS0618 — Member is obsolete**

            > {d.GetMessage()}

            **What it means:** You're using a method, property, or type that has been marked with `[Obsolete]`. The author is signaling it should no longer be used and may be removed in a future version.

            **Why it happens:**
            - A library or framework deprecated the API and recommends a newer alternative.
            - Internal code was marked obsolete during a refactor.

            **Common fixes:**
            1. Read the obsolete message — it usually suggests the replacement.
            2. Migrate to the recommended alternative API.
            3. If you must keep using it temporarily, suppress with `#pragma warning disable CS0618`.
            """;
    }

    // ── CS0131: Left side of assignment must be a variable, property, or indexer ──

    private static string ExplainCS0131(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS0131 — The left-hand side of an assignment must be a variable, property, or indexer**

            > {d.GetMessage()}

            **What it means:** You're trying to assign a value to something that can't be assigned to — like a method call result, a literal, or a read-only expression.

            **Why it happens:**
            - Assigning to a method call: `GetValue() = 5;`
            - Assigning to a literal: `42 = x;`
            - Assigning to the result of an expression, such as `a + b = c;`.

            **Common fixes:**
            1. Make sure the left side is a variable, property, or indexer.
            2. Move calculations to the right side of the assignment.
            3. If you meant to compare values in a condition, use `==` instead of assigning to an expression.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                // ❌ Error: can't assign to a method call
                GetName() = "Alice";

                // ❌ Error: can't assign to an arithmetic expression
                if (a + b = c) { }

                // ✅ Fix
                string name = GetName();
                if (a + b == c) { }
                ```
                """;
        }

        return md;
    }

    // ── CS0111: Type already defines a member with same parameter types ──

    private static string ExplainCS0111(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS0111 — Type already defines a member with the same parameter types**

            > {d.GetMessage()}

                **What it means:** A type declares two members that the compiler treats as the same signature. C# can't distinguish overloads by return type alone.

            **Why it happens:**
            - Copy-pasting a method without changing its parameters.
            - Two overloads differ only in return type (which doesn't count for overloading).
                - Parameter differences collapse to the same signature, such as `dynamic` vs `object`, nullable annotations, or `ref` vs `out`.

            **Common fixes:**
            1. Rename one of the methods.
                2. Change the parameter list so the signatures are distinct.
            3. Remove the duplicate if it was unintentional.

                Note: C# overloading is based on the member signature, not the return type.
            """;
    }

    // ── CS0236: A field initializer cannot reference the non-static field ──

    private static string ExplainCS0236(Diagnostic d, DiagnosticExplanationRequest req)
    {
        var md = $"""
            **CS0236 — A field initializer cannot reference the non-static field, method, or property**

            > {d.GetMessage()}

            **What it means:** You're trying to use an instance field, method, or property to initialize a field at declaration time. C# doesn't allow that dependency while the object is still being initialized.

            **Why it happens:**
            - Referencing `this` (implicitly) in a field initializer.
            - Using one field's value to compute another field's initial value.
            - Calling an instance method or property from a field initializer.

            **Common fixes:**
            1. Move the initialization into the constructor.
            2. Make the referenced field `static` if it doesn't need instance state.
            3. Use a property expression if the value should be computed from current instance state each time.
            """;

        if (req.IncludeExamples)
        {
            md += """

                **Example:**
                ```csharp
                class Config
                {
                    int basePort = 8080;
                    int apiPort = basePort + 1; // ❌ Error

                    // ✅ Fix: initialize in constructor
                    int apiPort;
                    public Config() => apiPort = basePort + 1;
                }
                ```
                """;
        }

        return md + RelatedConcept("static", "A `static` field belongs to the type and is initialized once. Static fields can be referenced in other field initializers.");
    }

    // ── CS0012: Type is defined in assembly that is not referenced ──

    private static string ExplainCS0012(Diagnostic d, DiagnosticExplanationRequest req)
    {
        return $"""
            **CS0012 — Type is defined in an assembly that is not referenced**

            > {d.GetMessage()}

            **What it means:** Your code uses a type that lives in a different assembly (DLL), but your project doesn't have a reference to that assembly.

            **Why it happens:**
            - A NuGet package or project reference is missing.
            - You upgraded a package and a transitive dependency was dropped.
            - The type moved to a different assembly in a newer version.

            **Common fixes:**
            1. Add the NuGet package: `dotnet add package PackageName`
            2. Add a project reference: `dotnet add reference ../OtherProject`
            3. Check which package provides the assembly and install it.
            """;
    }

    // ── Fallback for unknown diagnostic IDs ──

    private static string GetFallbackExplanation(Diagnostic diagnostic)
    {
        var severity = diagnostic.Severity switch
        {
            DiagnosticSeverity.Error => "Error",
            DiagnosticSeverity.Warning => "Warning",
            DiagnosticSeverity.Info => "Info",
            _ => "Message"
        };

        return $"""
            **{diagnostic.Id} — Compiler {severity}**

            > {diagnostic.GetMessage()}

            This diagnostic ID doesn't have a curated explanation yet, but here's how to proceed:

            **General steps:**
            1. Read the message carefully — it usually describes the exact problem.
            2. Search for **{diagnostic.Id}** in the [Microsoft C# docs](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/) for a detailed explanation.
            3. Check the code at the indicated location for typos, missing references, or type mismatches.

            **Severity:** {severity}
            """;
    }

    // ── Related concept helpers ──

    private static string RelatedConcept(string keyword, string description)
    {
        return $"""

            ---
            **Related concept — `{keyword}`**

            {description}
            """;
    }

    private static string RelatedConcepts(params (string keyword, string description)[] concepts)
    {
        var section = "\n\n---\n**Related concepts**\n";
        foreach (var (keyword, description) in concepts)
        {
            section += $"\n- **`{keyword}`** — {description}";
        }
        return section;
    }
}
