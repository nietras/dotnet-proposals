Currently, there is no way to binary search in sorted native or fixed memory (e.g. coming from a pointer) in .NET, this proposal intends to fix that by adding binary search extension methods to `ReadOnlySpan<T>` (and currently also `Span<T>` due to type inference issues), but also proposes some different overloads than seen on `Array` to allow for inlined comparisons via the possibility to use value type comparables and comparers.

### Proposed API
Add a set of `BinarySearch` extension methods for `ReadOnlySpan<T>` and `Span<T>` in `SpanExtensions`:
```csharp
    public static class SpanExtensions
    {
        // Convenience overload
        public static int BinarySearch<T>(
            this ReadOnlySpan<T> span, IComparable<T> comparable) 
        { return BinarySearch<T, IComparable<T>>(span, comparable); }

        public static int BinarySearch<T, TComparable>(
            this ReadOnlySpan<T> span, TComparable comparable) 
            where TComparable : IComparable<T> 
        { throw null; }

        public static int BinarySearch<T, TComparer>(
            this ReadOnlySpan<T> span, T value, TComparer comparer) 
            where TComparer : IComparer<T>
        { throw null; }

        // NOTE: Due to the less-than-ideal generic type inference 
        //       in the face of implicit conversions,
        //       we need the overloads taking Span<T>. 
        //       These simply forward to ReadOnlySpanExtensions.

        // Convenience overload
        public static int BinarySearch<T>(
            this Span<T> span, IComparable<T> comparable) 
        { return BinarySearch<T, IComparable<T>>(span, comparable); }

        public static int BinarySearch<T, TComparable>(
            this Span<T> span, TComparable comparable) 
            where TComparable : IComparable<T> 
        { return BinarySearch<T, TComparable>((ReadOnlySpan<T>)span, comparable); }

        public static int BinarySearch<T, TComparer>(
            this Span<T> span, T value, TComparer comparer) 
            where TComparer : IComparer<T>
        { return BinarySearch((ReadOnlySpan<T>)span, value, comparer); }
    }
```

### Rationale and Usage
Provide a safe yet fast way of binary searching of any type of contiguous memory; managed or unmanaged.

```csharp
public static class UsageForInt
{
    struct InlineableComparer : IComparer<int>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(int a, int b)
        {
            if (a == b) { return 0; }
            if (a < b) { return -1; }
            return 1;
        }
    }

    struct InlineableComparable : IComparable<int>
    {
        int m_value;
        
        public InlineableComparable(int value)
        {
            m_value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(int other)
        {
            if (m_value == other) { return 0; }
            if (m_value < other) { return -1; }
            return 1;
        }
    }

    public static void SpanBinarySearch()
    {
        var span = new Span<int>();

        //span.Sort();

        int valueToFind = 42;

        // Direct value
        var index = span.BinarySearch(valueToFind);

        // Inlineable struct comparer
        var comparer = new InlineableComparer();
        index = span.BinarySearch(valueToFind, comparer);

        // Inlineable struct comparable
        var comparable = new InlineableComparable(valueToFind);
        index = span.BinarySearch(comparable);
    }

    public static void ReadOnlySpanBinarySearch()
    {
        ReadOnlySpan<int> span = new Span<int>();

        //span.Sort();

        int valueToFind = 42;

        // Direct value
        var index = span.BinarySearch(valueToFind);

        // Inlineable struct comparer
        var comparer = new InlineableComparer();
        index = span.BinarySearch(valueToFind, comparer);

        // Inlineable struct comparable
        var comparable = new InlineableComparable(valueToFind);
        index = span.BinarySearch(comparable);
    }
}

public static class UsageForCompound
{
    struct Compound
    {
        public float FeatureValue;
        public int FeatureIndex;
        public object Payload;
    }

    struct InlineableFeatureValueComparer : IComparer<Compound>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(Compound a, Compound b)
        {
            if (a.FeatureValue == b.FeatureValue) { return 0; }
            if (a.FeatureValue < b.FeatureValue) { return -1; }
            return 1;
        }
    }

    struct InlineableFeatureComparable : IComparable<Compound>
    {
        readonly float m_featureValue;
        
        public InlineableFeatureComparable(float featureValue)
        {
            m_featureValue = featureValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Compound other)
        {
            if (m_featureValue == other.FeatureIndex) { return 0; }
            if (m_featureValue < other.FeatureIndex) { return -1; }
            return 1;
        }
    }

    public static void SpanBinarySearch()
    {
        var span = new Span<Compound>();

        //span.Sort(new InlineableFeatureValueComparer());

        float featureValueToFind = 1.234f;

        // Inlineable struct comparer
        var comparer = new InlineableFeatureValueComparer();
        // Less than ideal for compound, which is reason for comparable overload
        var compound = new Compound(){ FeatureValue = featureValueToFind };
        var index = span.BinarySearch(compound, comparer);

        // Inlineable struct comparable (easier to use for compounded type)
        var comparable = new InlineableFeatureComparable(featureValueToFind);
        index = span.BinarySearch(comparable);
    }

    public static void ReadOnlySpanBinarySearch()
    {
        ReadOnlySpan<Compound> span = new Span<Compound>();

        //span.Sort(new InlineableFeatureValueComparer());

        float featureValueToFind = 1.234f;

        // Inlineable struct comparer
        var comparer = new InlineableFeatureValueComparer();
        // Less than ideal for compound, which is reason for comparable overload
        var compound = new Compound(){ FeatureValue = featureValueToFind };
        var index = span.BinarySearch(compound, comparer);

        // Inlineable struct comparable (easier to use for compounded type)
        var comparable = new InlineableFeatureComparable(featureValueToFind);
        index = span.BinarySearch(comparable);
    }
}
```

The argumentation for adding this is:
 * To increase the efficiency of code doing binary searching and prevent people from reinventing the wheel.
 * Allow binary searching on contiguous memory of any kind.

### Open Questions
An important question regarding this proposal is whether the pattern with generic parameter `TComparer` (e.g. constrained to `where TComparer : IComparer<T>`) or `TComparable` (constrained to `where TComparable : IComparable<T>`) is a pattern that can be approved. This pattern allows for inlineable comparables/comparers at the cost of increased code size, if no value type comparables/comparers are used, there should be no difference. This pattern is also used in the proposal for `Sort` in https://github.com/dotnet/corefx/issues/15329, that has been approved.

Another open question is whether the overload taking `IComparable<T>` is necessary.

The API relies on being able to depend upon `System.Collections.Generic`, could this be an issue?

@karelz @jkotas @KrzysztofCwalina @jamesqo

### Updates
UPDATE 1: Add link to Sort and point on the pattern used.
UPDATE 2: Add IComparable<T> overloads for convenience as suggested by @jkotas
UPDATE 3: Combine all extensions into `SpanExtensions`.

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
