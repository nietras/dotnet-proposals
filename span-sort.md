Currently, there is no way to sort native or fixed memory 
(e.g. coming from a pointer) in .NET, this proposal wants to fix that 
by adding sorting methods to `Span<T>`, but also proposes some different 
overloads than seen on `Array` to allow for inlined comparisons via 
the possibility to use value type comparers.

### Rationale and Usage
Provide a safe yet fast way of sorting of any type of contiguous memory; managed or unmanaged.

```csharp
var span = new Span<int>(ptr, length);
span.Sort(); // Sort elements in native memory
```

// TODO: More examples, especially, non-alloc, inline compares

The argumentation for adding this is to:
 * To increase the efficiency of code doing this and prevent people from reinventing the wheel.
 * Allow performance optimizations depending on memory type and contents.

### Proposed API
Add different `Sort` methods to the existing `Span<T>` API:
```csharp
public class Span<T>
{
     public void Sort();
     public void Sort<TComparer>(TComparer comparer) where TComparer : IComparer<T>;
     public void Sort(System.Comparison<T> comparison); // Convenience overload e.g. forwards to generic TComparer version, via struct DelegateComparer : IComparer<T>
     
     public void Sort<TValue>(Span<TValue> items);
     public void Sort<TValue, TComparer>(Span<TValue> items, TComparer comparer) where TComparer : IComparer<T>;
     public void Sort<TValue>(Span<TValue> items, System.Comparison<T> comparison); // Convenience overload e.g. forwards to generic TComparer version, via struct DelegateComparer : IComparer<T>
}
```
Alternatively, static methods could be added to non-generic static class `Span`, that does not exist currently.

### Open Questions
Open question is whether this should be added as member methods or static class methods like in `Array`. 
I would argue for a member methods, since this will depend on internal representation, 
and would perhaps allow for the JIT intrinsic version to do optimizations that a static extension method can't. 
If that is a valid argument?

@karelz @jkotas @omariom @benaadams @jamesqo 

### Static Array.Sort methods
https://github.com/dotnet/corefx/blob/master/src/System.Runtime/ref/System.Runtime.cs

```csharp
public static void Sort(System.Array array) { }
public static void Sort(System.Array keys, System.Array items) { }
public static void Sort(System.Array keys, System.Array items, System.Collections.IComparer comparer) { }
public static void Sort(System.Array keys, System.Array items, int index, int length) { }
public static void Sort(System.Array keys, System.Array items, int index, int length, System.Collections.IComparer comparer) { }
public static void Sort(System.Array array, System.Collections.IComparer comparer) { }
public static void Sort(System.Array array, int index, int length) { }
public static void Sort(System.Array array, int index, int length, System.Collections.IComparer comparer) { }
public static void Sort<T>(T[] array) { }
public static void Sort<T>(T[] array, System.Collections.Generic.IComparer<T> comparer) { }
public static void Sort<T>(T[] array, System.Comparison<T> comparison) { }
public static void Sort<T>(T[] array, int index, int length) { }
public static void Sort<T>(T[] array, int index, int length, System.Collections.Generic.IComparer<T> comparer) { }
public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items) { }
public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, System.Collections.Generic.IComparer<TKey> comparer) { }
public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, int index, int length) { }
public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, int index, int length, System.Collections.Generic.IComparer<TKey> comparer) { }
```

### List.Sort methods
TODO

Find other examples...