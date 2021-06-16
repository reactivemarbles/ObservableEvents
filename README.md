# Observable event generator

This project is a .NET 5 source generator which produces `IObservable<T>` for events contained within a object including all base classes.

# Installation

## NuGet Packages

Make sure your project is using the newer `PackageReference` inside your CSPROJ. The older style is buggy and should be moved away from regardless. See here for discussions how to [upgrade](https://docs.microsoft.com/en-us/nuget/consume-packages/migrate-packages-config-to-package-reference).

Install the following packages to start using Observable Events.

| Name                          | Platform          | NuGet                            |
| ----------------------------- | ----------------- | -------------------------------- |
| [ReactiveMarbles.ObservableEvents.SourceGenerator][Core]       | Core - Libary     | [![CoreBadge]][Core]             |


[Core]: https://www.nuget.org/packages/ReactiveMarbles.ObservableEvents.SourceGenerator/
[CoreBadge]: https://img.shields.io/nuget/v/ReactiveMarbles.ObservableEvents.SourceGenerator.svg

## What does it do?

ObservableEvents generator will convert events within an assembly and create observable wrappers for them. 

It is based on pharmacist [Pharmacist](https://github.com/reactiveui/Pharmacist) and uses .NET Source Generator technology.

## Installation
Include the following in your .csproj file

```xml
<PackageReference Include="ReactiveMarbles.ObservableEvents.SourceGenerator" Version="1.0.2" PrivateAssets="all" />
```

The `PrivateAssets` will prevent the ObservableEvents source generator from being inherited by other projects.

## How to use

### Instance Based
It injects a class for instance based events into your source code which will expose a extension method called `Events()`.

You need to include the namespace `ReactiveMarbles.ObservableEvents` to access to the extension method.

You can then use this to get `IObservable<T>` instances from your events.

```cs
using ReactiveMarbles.ObservableEvents;

public void MyClass : INotifyPropertyChanged
{
  // Assumes this belong in a class with a event called PropertyChanged.
  public void RunEvents()
  {
      this.Events().PropertyChanged.Subscribe(x => Console.WriteLine($"The {x} property has changed"));
  }

  public event PropertyChangedEventHandler PropertyChanged;
}
```

### Static Events

You must use include a attribute `GenerateStaticEventObservables` on the assembly level for a particular class. This will generate a class `RxEvents` in the same namespace as the specified class.

```cs
[assembly: GenerateStaticEventObservablesAttribute(typeof(StaticTest))]

    public static class StaticTest
    {
        public static event EventHandler? TestChanged;
    }
```
