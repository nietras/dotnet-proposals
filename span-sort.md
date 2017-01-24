Currently, there is no way to sort native or fixed memory (e.g. coming from a pointer) in .NET, this proposal intends to fix that by adding sorting extension methods to `Span<T>`, but also proposes some different overloads than seen on `Array` to allow for inlined comparisons via the possibility to use value type comparers.

### Proposed API
Add a set of `Sort` extension methods to `Span<T>` in `SpanExtensions`:
```csharp
public static class SpanExtensions
{
     public static void Sort<T>(this Span<T> span);

     public static void Sort<T, TComparer>(this Span<T> span, TComparer comparer) 
        where TComparer : IComparer<T>;

     public static void Sort<T>(this Span<T> span, System.Comparison<T> comparison);
     
     public static void Sort<TKey, TValue>(this Span<TKey> keys, Span<TValue> items);

     public static void Sort<TKey, TValue, TComparer>(this Span<TKey> keys, 
        Span<TValue> items, TComparer comparer) 
        where TComparer : IComparer<TKey>;
        
     public static void Sort<TKey, TValue>(this Span<TKey> keys, 
        Span<TValue> items, System.Comparison<TKey> comparison);
}
```

### Rationale and Usage
Provide a safe yet fast way of sorting of any type of contiguous memory; managed or unmanaged.

#### Sorting Native Memory
```csharp
var span = new Span<int>(ptr, length);
span.Sort(); // Sort elements in native memory
```
#### Sorting with Inlineable Comparer
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

Span<int> span = GetSomeSpan();
// Sort elements, in reverse order with inlined Compare,
// without heap allocation for comparer
span.Sort(new ReverseComparer()); 
```
#### Sorting based on Lambda
```csharp
Span<int> span = GetSomeSpan();
// Sort elements, in reverse order with lambda/delegate
span.Sort((a, b) => a == b ? 0 : (a > b ? -1 : 1)); 
```

The argumentation for adding this is:
 * To increase the efficiency of code doing sorting and prevent people from reinventing the wheel.
 * Allow performance optimizations depending on memory type and contents.
 * Allow sorting on contiguous memory of any kind.

#### Use Cases
In almost any domain where a high volume of data is processed with sorting being one operation and memory management (e.g. memory recycling, buffer pooling, native memory) is a must to achieve high performance with minimal latency, these sorts would be useful. Example domains are database engines (think indexing), computer vision, artificial intelligence etc.

A concrete example could be in the training of Random Forests some methods employ feature sorting (with indeces) to find decision boundaries on. This involves a lot of data and data that can originate from unmanaged memory.

### Open Questions
The API relies on being able to depend upon `System.Collections.Generic`, could this be an issue?

@karelz @jkotas @jamesqo 

### Updates
UPDATE 1: Change API to be defined as extension methods.


### Existing Sort APIs
A non-exhaustive list of existing sorting APIs is given below for comparison.

#### `Array.Sort` Static Methods
Found in [ref/System.Runtime.cs](https://github.com/dotnet/corefx/blob/master/src/System.Runtime/ref/System.Runtime.cs)

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
Found in [ref/System.Collections.cs](https://github.com/dotnet/corefx/blob/master/src/System.Collections/ref/System.Collections.cs)

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
Found in [ref/System.Linq.cs](https://github.com/dotnet/corefx/blob/master/src/System.Linq/ref/System.Linq.cs)

```csharp
public static System.Linq.IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this System.Collections.Generic.IEnumerable<TSource> source, System.Func<TSource, TKey> keySelector) { throw null; }
public static System.Linq.IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this System.Collections.Generic.IEnumerable<TSource> source, System.Func<TSource, TKey> keySelector, System.Collections.Generic.IComparer<TKey> comparer) { throw null; }
public static System.Linq.IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this System.Collections.Generic.IEnumerable<TSource> source, System.Func<TSource, TKey> keySelector) { throw null; }
public static System.Linq.IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this System.Collections.Generic.IEnumerable<TSource> source, System.Func<TSource, TKey> keySelector, System.Collections.Generic.IComparer<TKey> comparer) { throw null; }
```