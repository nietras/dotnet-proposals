Currently, there is no way to binary search in sorted native or fixed memory (e.g. coming from a pointer) in .NET, this proposal intends to fix that by adding binary search extension methods to `ReadOnlySpan<T>`, but also proposes some different overloads than seen on `Array` to allow for inlined comparisons via the possibility to use value type comparers.

### Proposed API
Add a set of `BinarySearch` extension methods to `ReadOnlySpan<T>` in `ReadOnlySpanExtensions`:
```csharp
public static class ReadOnlySpanExtensions
{
        public static int BinarySearch<T, TComparable>(this ReadOnlySpan<T> span, TComparable comparable) 
            where TComparable : IComparable<T> 
        { throw null; }
        
        public static int BinarySearch<T, TComparer>(this ReadOnlySpan<T> span, T value, TComparer comparer) 
            where TComparer : IComparer<T>
        { throw null; }
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

@karelz @jkotas @KrzysztofCwalina @jamesqo @AtsushiKan 

### Updates


### Existing Sort APIs
A non-exhaustive list of existing binary search APIs is given below for comparison.

#### `Array.BinarySearch` Static Methods
Found in [ref/System.Runtime.cs](https://github.com/dotnet/corefx/blob/master/src/System.Runtime/ref/System.Runtime.cs)

```csharp
public static int BinarySearch(System.Array array, int index, int length, object value) { throw null; }
public static int BinarySearch(System.Array array, int index, int length, object value, System.Collections.IComparer comparer) { throw null; }
public static int BinarySearch(System.Array array, object value) { throw null; }
public static int BinarySearch(System.Array array, object value, System.Collections.IComparer comparer) { throw null; }
public static int BinarySearch<T>(T[] array, T value) { throw null; }
public static int BinarySearch<T>(T[] array, T value, System.Collections.Generic.IComparer<T> comparer) { throw null; }
public static int BinarySearch<T>(T[] array, int index, int length, T value) { throw null; }
public static int BinarySearch<T>(T[] array, int index, int length, T value, System.Collections.Generic.IComparer<T> comparer) { throw null; }
```

#### `List<T>.BinarySearch` Member Methods
Found in [ref/System.Collections.cs](https://github.com/dotnet/corefx/blob/master/src/System.Collections/ref/System.Collections.cs)

```csharp
public partial class List<T> : System.Collections.Generic.ICollection<T>, System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IList<T>, System.Collections.Generic.IReadOnlyCollection<T>, System.Collections.Generic.IReadOnlyList<T>, System.Collections.ICollection, System.Collections.IEnumerable, System.Collections.IList
{
    public int BinarySearch(T item) { throw null; }
    public int BinarySearch(T item, System.Collections.Generic.IComparer<T> comparer) { throw null; }
    public int BinarySearch(int index, int count, T item, System.Collections.Generic.IComparer<T> comparer) { throw null; }
}
```
