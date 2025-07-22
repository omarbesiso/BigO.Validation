# BigO.Validation

##TL;DR

**BigO.Validation** is a *tiny* but **surgical** guard/validation helper for .NET 9+.
 It offers:

- **Throw‑helpers** (`Guard.*`) that *inline* cheap checks and centralise the actual exception throw so your JITted code stays lean ([Microsoft for Developers](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-8/))
- **Property‑level helpers** (`PropertyGuard.*`) for DDD style value objects and records
- **Attribute‑free** API *and* optional **DataAnnotations** integration when you need it ([Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=net-9.0))
- Zero dependencies, MIT‑licensed, 100 % C# ([GitHub](https://github.com/omarbesiso/BigO.Validation))
- A **single 30 KB IL** payload – small enough that you’ll never notice it in your container image.

------

## Table of Contents

1. [Features](#features)
2. [Installation](#installation)
3. [Quick‑start](#quick-start)
4. [Guard API](#guard-api)
5. [PropertyGuard API](#propertyguard-api)
6. [DataAnnotations Extensions](#dataannotations-extensions)
7. [Design Notes](#design-notes)
8. [Road‑map](#road-map)
9. [Contributing](#contributing)
10. [License](#license)

------

## Features

### One‑liner argument guards

All core rules are exposed as static methods so you can write:

```csharp
Guard.NotNullOrWhiteSpace(name);
Guard.WithinRange(age, 0, 120);
Guard.EmailAddress(email);
```

The current rule‑set covers **20+ scenarios** including null/empty checks, length & range checks, regex, e‑mail, URL and temporal rules like *InPast / InFuture*.

### Property‑level validation

Every `Guard.*` has a sibling `PropertyGuard.*` that is tailor‑made for use inside property setters so you can keep your entities honest without sprinkling attributes everywhere.

### Zero‑alloc throw helpers

Validation passes are inlined; if a rule fails the flow jumps to a single non‑inlined helper that actually throws – identical to `ArgumentNullException.ThrowIfNull` introduced in .NET 6 ([Microsoft for Developers](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-8)).

### Optional DataAnnotations bridge

If you can’t abandon attributes (e.g. ASP.NET MVC model binding) you can call `AnnotationValidator.ValidateAndThrow(obj)` to execute the standard `Validator.TryValidateObject` pipeline behind the scenes ([Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.validator?view=net-9.0)).

### Extensible & discoverable

All public APIs live in `BigO.Validation` so IntelliSense surfaces the whole guard catalogue.
 Roll‑your‑own rules via `Guard.Requires(predicate, message)` or by adding another static partial file.

------

## Installation

### NuGet (recommended)

```powershell
dotnet add package BigO.Validation
```

No packages yet? Push it with:

```powershell
dotnet pack -c Release
dotnet nuget push bin/Release/BigO.Validation.*.nupkg --source <your feed>
```

### Local reference

```xml
<ProjectReference Include="src/BigO.Validation/BigO.Validation.csproj" />
```

Requires SDK‑style projects (.NET 6+) – if you are still on legacy csproj, it’s 2025 – migrate.

------

## Quick‑start

```csharp
using BigO.Validation;

// constructor guard
public User(string email, int age)
{
    Guard.EmailAddress(email);
    Guard.WithinRange(age, 0, 120);
    (Email, Age) = (email, age);
}

// property guard
private string _nickname = "";
public string Nickname
{
    get => _nickname;
    set => _nickname = PropertyGuard.NotNullOrWhiteSpace(value);
}

// DataAnnotations bridge – validate DTO before persisting
public static void Save(ProfileDto dto)
{
    AnnotationValidator.ValidateAndThrow(dto); // throws ValidationException on failure
    _repo.Save(dto);
}
```

ASP.NET Core automatically runs DataAnnotations during model binding ([Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-9.0)),
 so for web controllers you normally stick to guard methods in your domain layer and let the framework handle request DTOs.

------

## Guard API

| Method                                                       | Success criteria                    | Exception on failure          |
| ------------------------------------------------------------ | ----------------------------------- | ----------------------------- |
| `NotNull`                                                    | value != null                       | `ArgumentNullException`       |
| `NotEmpty` / `NotNullOrEmpty`                                | `Count`/`Length` > 0                | `ArgumentException`           |
| `NotNullOrWhiteSpace`                                        | `!string.IsNullOrWhiteSpace(value)` | `ArgumentException`           |
| `NonZero`                                                    | number ≠ 0                          | `ArgumentOutOfRangeException` |
| `Positive`                                                   | number > 0                          | `ArgumentOutOfRangeException` |
| `ExactLength(n)`                                             | `Length == n`                       | `ArgumentOutOfRangeException` |
| `MinLength(n)` / `MaxLength(n)` / `LengthWithinRange(min,max)` | obvious                             | `ArgumentOutOfRangeException` |
| `Minimum(n)` / `Maximum(n)` / `WithinRange(min,max)`         | obvious                             | `ArgumentOutOfRangeException` |
| `MatchesRegex(pattern)`                                      | `Regex.IsMatch(value)`              | `ArgumentException`           |
| `EmailAddress`                                               | RFC‑5322 light regex                | `ArgumentException`           |
| `Url`                                                        | `Uri.TryCreate(value, ...)`         | `ArgumentException`           |
| `InPast` / `InFuture`                                        | temporal checks                     | `ArgumentOutOfRangeException` |
| `Requires(predicate)`                                        | custom                              | `ArgumentException`           |

------

## PropertyGuard API

`PropertyGuard` mirrors `Guard` but **returns the value** for fluent assignment:

```csharp
_someField = PropertyGuard.NotNull(value);
```

Under the hood it simply forwards to the same throw‑helpers; no extra allocations.

------

## DataAnnotations Extensions

```csharp
using BigO.Validation.DataAnnotations;

AnnotationValidator.Validate(obj);               // returns ValidationResult[]
AnnotationValidator.ValidateAndThrow(obj);       // throws ValidationException
```

Internally this bridges to `System.ComponentModel.DataAnnotations.Validator` ([Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.validator?view=net-9.0)).
 If you’re authoring custom attributes remember they must derive from `ValidationAttribute` ([Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.validationattribute?view=net-9.0)).

> **Tip** Use RuleSets or FluentValidation if you need conditional or composable rules ([docs.fluentvalidation.net](https://docs.fluentvalidation.net/en/latest/rulesets.html)) – BigO.Validation deliberately stays simple.

------

## Design Notes

### Why extension‑method guards and not attributes?

- **Call‑site clarity** – you see the rule right next to the argument, not somewhere in the class definition.
- **Zero reflection** at runtime – attributes require `Validator.TryValidateObject` which walks metadata and boxes values ([Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions-1/models-data/validation-with-the-data-annotation-validators-cs)).
- **No nonsense on value objects** – a `Money` struct shouldn’t carry UI annotations.

### Performance

- Fast path gets fully inlined; slow path is a single call into `ThrowHelper` so the JIT can optimise happy paths ([Microsoft for Developers](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-8/)).
- Argument name is captured via `[CallerArgumentExpression]` to avoid hard‑coding parameter strings – same trick used by BCL throw‑helpers ([Microsoft for Developers](https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7/)).

### FluentValidation?

Nothing wrong with it (see official docs) ([docs.fluentvalidation.net](https://docs.fluentvalidation.net/en/latest/including-rules.html)), but if you want *zero dependencies* and prefer imperative validation, BigO.Validation is for you. You can always wrap a FluentValidator inside `Guard.Requires`.

------

## License

MIT – do whatever you want but don’t blame us if you shoot yourself in the foot ([GitHub](https://github.com/omarbesiso/BigO.Validation)).

------

Happy validating! 🎉