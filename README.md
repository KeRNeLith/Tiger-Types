# Tiger.Types

## What It Is

Tiger.Types is a library of useful types for C#, ones that are sometimes included by default in other languages.  These types enable and include advanced operations that encapaulate boilerplate logic.  These include, but are not limited to:

- The type `Option<TSome>`, which represents the concept of "a value" or "no value" in a way that is more type-safe than returning `null`.  This maps to failable operations where failure delivers no specific information.
- The type `Either<TLeft, TRight>`, which represents the concept of "a value" or "a different value" in a way that is more type-safe than always throwing an exception.  This maps to operations that can return a value upon success, or a detailed error upon failure.
- Advanced operations on `Task<T>`, which allow you to transform values while remaining within the `Task<T>` context.

## Why You Want It

These types and operations allow you to treat more operations in your .NET application as [monads](https://en.wikipedia.org/wiki/Monad_\(functional_programming\)#Motivating_examples), which frequently represent operations in more type-safe ways than .NET conventions.

Let's use `Option<TSome>` for a short example.  In the following code, we'll write a short method that converts a string to all upper-case.

```
public string ToAllUpperCase(string input)
{
  return input.ToUpper();
}
```

(It is a trivial example, since the capability is built into the `string` type, but it will do.)

There is already a somewhat major error that could occur: If `input` is `null`, then the method will throw `NullReferenceException`.  This is a bug, and the type system did nothing to help you detect it.  Your method can accept any `string` value *and* the kind-of-value `null`.  We can check for `null`, and throw `ArgumentNullException`, but let's say that we need to allow the concept of "no value" into the method.

```
public string ToAllUpperCase(string input)
{
  if (input == null)
  {
    return null;
  }
  return input.ToUpper();
}
```

This looks better, but is even worse!  Your callers still believe that you can only return actual `string` values, but you can also return `null`.  Now you are checking for `null`, *and* your callers must check, as well.  This is where `Option<TSome>` comes in.  Similar to the .NET concept of `Nullable<T>` (also written as `T?`), `Option<TSome>` allows you to represent the concept of "no value", and explicitly advertise it to your callers.

The pattern that emerges is this: If we get no value, return no value.  If we get a value, we process it.  This pattern is built into the type, and it is called `Map`.  That allows us to write the original verison of `ToAllUpperCase` defined above, the one that operates only on values, and call it like this:

```
potentialString.Map(s => ToAllUpperCase(s));
```

(Of course, your caller could also write `potentialString.Map(s => s.ToUpper())`, but then you're out of a job!)

This is almost identical to `Select` on `IEnumerable<T>`:  If the sequence is empty, we get back an empty sequence.  If the sequence has elements, the elements are transformed.  For `Option<TSome>` the "empty" state is called `None`, and the "has elements" state is called `Some`.  There are many such useful operations on optional values.  Here's an abridged list.

- `Map`: Given a value of `Func<TSome, U>`, returns an `Option<U>` in the same state as the input value. <small>Aliased to `Select`, from the BCL.</small>

- `Bind`: Given a value of `Func<TSome, Option<U>>`, calculates an `Option<Option<U>>` and flattens it to `Option<U>` before returning. <small>Aliased to `SelectMany`, from the BCL.</small>

- `Match` <small>(Value-Returning)</small>: Associates a value of `Func<U>` with the `None` state and a value of `Func<TSome, U>` with the `Some` state, and invokes the function that matches the input value's state.

- `Match` <small>(Action-Performing)</small>: Associates a value of `Action<U>` with the `None` state and a value of `Action<TSome>` with the `Some` state, and invokes the function that matches the input value's state.

- `Filter`: Given a predicate value of `Func<TSome, bool>`, returns the original value if it is in the `Some` state and its `Some` value passes the predicate; otherwise, returns an `Option<TSome>` in the `None` state. <small>Aliased to `Where`, from the BCL.</small>

- `Tap`: Given a value of `Action<TSome>`, performs that action if the original value is in the `Some` state, then returns the original value -- most useful for chained methods. <small>Aliased to `Do`, from the Interactive Extensions.</small>

- `Let`: Given a value of `Action<TSome>`, performs that action if the original value is in the `Some` state. <small>Aliased to `ForEach`, from the Interactive Extensions.</small>

### A Note on Aliases

Many of the methods on these types are aliased to LINQ-standard names.  This is for reasons of developer familiarity and activating certain C# features.  For example, implementing `Select`, `SelectMany` and `Where` allows the LINQ query syntax to be used.  For example, using `Option<T>` again:

```
var left = Option.From(3); // Some(3)
var right = Option.From(4); // Some(4)

var sum = from l in left
          from r in right
          select l + r; // Some(7)
```

However, if either of the input values is in the `None` state, the operation fails.

```
var left = Option<int>.None; // None
var right = Option.From(4); // Some(4)

var sum = from l in left
          from r in right
          select l + r; // None
```

Additionally, implementing `GetEnumerator` allows an `Option<TSome>` to be used with the `foreach` statement, which will execute its body only if the optional value is in the `Some` state.

```
foreach (var value in Option.From("world"))
{
  Console.WriteLine($"Hello, {value}!"); // "Hello, world!"
}

foreach (var value in Option<string>.None)
{
  Console.WriteLine($"Hello, {value}!"); // <not executed>
}
```

### A Note on `null`

Most of this library is allergic to `null`.  It advertises where `null` is allowed, and where it is not -- heavily tilted to the latter.  If returning `null` from a passed function would violate the semantics of an operation, then that method will thow an exception.  For example, the contract of `Map` is that it will only return an optional value in the `None` state if the original value is in the `None` state.  However, if returning `null` from the passed function were allowed, that would put the returned value into the `None` state.  This should be refactored into a method of type `Func<TSome, Option<U>>`, and used with the `Bind` operation.

## How You Develop It

You're in the right place for that.  Once you have this directory forked and cloned, the provided Visual Studio solution file should contain everything you need to get going.  The NuGet packages will be restored and the NUnit unit tests will be detected (if your version of Visual Studio supports NUnit3, that is).  If you're interested in the command-line builds, they use a system called [Cake](http://cakebuild.net).  While it is possible to run the cakefile (`build.cake`) directly, the preferred method is to run the build bootstrapper (`build.ps1`).  The build bootstrapper ensures that you have the development and testing tools installed in your environment.  It is a powershell script, so the way to execute it will vary by your command line.

- Powershell: `./build.ps1`
- cmd: `powershell ./build.ps1`
- bash: `powershell ./build.ps1`

This repository is attempting to use the [GitFlow](http://jeffkreeftmeijer.com/2010/why-arent-you-using-git-flow/) branching methodology.  Results may be mixed, please be aware.

### A Note on Code Contracts

This codebase uses Microsoft's [Code Contracts](https://github.com/Microsoft/CodeContracts) libraries to provide static analysis of pre- and post-conditions.  If you also activate this feature, you will be told at compile-time if you could violate one of these constraints.  If you do not activate this feature, then exceptions such as `ArgumentNullException` will be thrown at runtime in the usual fashion.

## Thank You

Seriously, though.  Thank you for using this software.  The author hopes it performs admirably for you.