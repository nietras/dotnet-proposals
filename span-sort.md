Currently, there is no way to sort native or fixed memory 
(e.g. coming from a pointer) in .NET, this proposal intends to fix that 
by adding sorting methods to `Span<T>`, but also proposes some different 
overloads than seen on `Array` to allow for inlined comparisons via 
the possibility to use value type comparers.

### Rationale and Usage
Provide a safe yet fast way of sorting of any type of contiguous memory; managed or unmanaged.

```csharp
var span = new Span<int>(ptr, length);
span.Sort(); // Sort elements in native memory
```

```csharp
struct ReverseComparer : IComparer<int>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(int a, int b)
    {
        if (a == b) { return 0; }
        if (a > b) { return -1; }
        return 1;
    }
}

var nativeSpan = new Span<int>(ptr, length);
// Sort elements in native memory, in reverse order with inlined Compare,
// without heap allocation
nativeSpan.Sort(new ReverseComparer()); 

var a = new int[1024];
// ... fill a
var managedSpan = new Span<int>(a);
// Sort elements in managed memory, in reverse order with inlined Compare,
// without heap allocation
managedSpan.Sort(new ReverseComparer()); 
```

// TODO: More examples, especially, non-alloc, inline compares

The argumentation for adding this is to:
 * To increase the efficiency of code doing this and prevent people from reinventing the wheel.
 * Allow performance optimizations depending on memory type and contents.
 * Allow sorting on contiguous memory of any kind.

### Proposed API
Add a set of `Sort` methods to the existing `Span<T>` API:
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
I would argue for member methods, since this might depend on internal representation (e.g. managed or unmanaged memory), 
and would perhaps allow for the JIT intrinsic version to do optimizations that a static extension method can't. 

@karelz @jkotas @omariom @benaadams @jamesqo 

### Existing Sort APIs
A non-exhaustive list of existing sorting APIs is given below.

#### `Array.Sort` Static Methods
Found in https://github.com/dotnet/corefx/blob/master/src/System.Runtime/ref/System.Runtime.cs

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

#### `List<T>.Sort` Member Methods
Found in https://github.com/dotnet/corefx/blob/master/src/System.Collections/ref/System.Collections.cs

```csharp
public partial class List<T> : System.Collections.Generic.ICollection<T>, System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IList<T>, System.Collections.Generic.IReadOnlyCollection<T>, System.Collections.Generic.IReadOnlyList<T>, System.Collections.ICollection, System.Collections.IEnumerable, System.Collections.IList
{
    public void Sort() { }
    public void Sort(System.Collections.Generic.IComparer<T> comparer) { }
    public void Sort(System.Comparison<T> comparison) { }
    public void Sort(int index, int count, System.Collections.Generic.IComparer<T> comparer) { }
}
```

#### LINQ `OrderBy` Extension Methods
Found in https://github.com/dotnet/corefx/blob/master/src/System.Linq/ref/System.Linq.cs 

```csharp
public static System.Linq.IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this System.Collections.Generic.IEnumerable<TSource> source, System.Func<TSource, TKey> keySelector) { throw null; }
public static System.Linq.IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this System.Collections.Generic.IEnumerable<TSource> source, System.Func<TSource, TKey> keySelector, System.Collections.Generic.IComparer<TKey> comparer) { throw null; }
public static System.Linq.IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this System.Collections.Generic.IEnumerable<TSource> source, System.Func<TSource, TKey> keySelector) { throw null; }
public static System.Linq.IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this System.Collections.Generic.IEnumerable<TSource> source, System.Func<TSource, TKey> keySelector, System.Collections.Generic.IComparer<TKey> comparer) { throw null; }
```