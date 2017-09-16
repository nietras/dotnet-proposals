This proposal adds utility methods for checking spans for overlaps 
or if a span is contained in another span.
For previous discussions see https://github.com/dotnet/corefx/issues/17793,
 https://github.com/dotnet/corefxlab/issues/827 and 
https://github.com/dotnet/corefx/issues/18750. 
And a closed PR https://github.com/dotnet/corefx/pull/18731.

### Proposed API
Add two set of  extension methods `Contains` and `Overlaps` for `ReadOnlySpan<T>` in `SpanExtensions`:
```csharp
public static class SpanExtensions
{
    public static bool Overlaps<T>(this ReadOnlySpan<T> first, ReadOnlySpan<T> second);
    public static bool Overlaps<T>(this ReadOnlySpan<T> first, ReadOnlySpan<T> second, out int elementIndex);
    public static bool Contains<T>(this ReadOnlySpan<T> first, ReadOnlySpan<T> second);
    public static bool Contains<T>(this ReadOnlySpan<T> first, ReadOnlySpan<T> second, out int elementIndex);}
```
In case of the `second` not matching the alignment of `first` span 
or the alignment of type `T` an `ArgumentException` is thrown.

The `out int elementIndex` is the relative index of the start of 
the `second` span to the start pf the `first` span.

### Rationale and Usage
For `Overlaps` the scenario can involve algorithms transform elements from `first` to `second` span, 
if the spans overlap the result might be wrong depending on the algorithm. To be able to detect this
the `Overlaps` method can be called.

For `Contains` the scenario can involve buffer pool management, where spans are returned from a buffer
and later needs to be returned to a given buffer. Calling `Contains` allows checking for which
buffer contains a given span.

#### Expected Results
TODO: Make table of expected results for the different cases:

 - **A**: `first` entirely before `second`, no overlap
```
first:   [--------)
         xRef     xRef + xLength
second:           [------------)     
                  yRef         yRef + yLength
```
 - **B**: `first` starts before `second` and ends inside `second`
```
first:   [------------)
         xRef         xRef + xLength
second:              [------------)     
                     yRef         yRef + yLength
```
 - **C**: `first` is entirely contained in `second`
```
first:       [-----------------------)
             xRef                    xRef + xLength
second:    [--------------------------)     
           yRef                       yRef + yLength
```
 - **D**: `first` starts inside target `second` and ends after `second` end
```
first:            [------)
                  xRef   xRef + xLength
second:    [-------)
           yRef   .            yRef + yLength
```
 - **E**: `first` entirely after `second`, no overlap
```
first:            [------------)     
                  xRef         xRef + xLength
second:  [--------)
         yRef     yRef + yLength
```
 - **F**: `first` starts before `second` and 
   ends after `second`, i.e. `second` is contained in `first`
```
first:   [-------------------------------)
         xRef                            xRef + xLength
second:    [--------------------------)     
           yRef                       yRef + yLength
```
 - **G**: `first` is same as `second`
```
first:   [--------------------------)
         xRef                       xRef + xLength
second:  [--------------------------)     
         yRef                       yRef + yLength
```

In table below `Overlaps => first.Overlaps(second)` or `Contains => first.Contains(second)`. `x => first` and `y => second`.
Needs to be reviewed.
|        |`Overlaps` |`Contains` |`` | 
|--------|-----------|-----------|---------------|
|**A**   |`false`    |`false`    |`elementIndex >= xLength`   |
|**B**   |`true`     |`false`    |`elementIndex > 0 && elementIndex < xLength`    |
|**C**   |`true`     |`false`    |`elementIndex <= 0`    |
|**D**   |`true`     |`false`    |`elementIndex < 0 && elementIndex > -yLength` |
|**E**   |`false`    |`false`    |`elementIndex < (-yLength + 1)` |
|**F**   |`true`     |`false`    |`elementIndex < xLength`    |
|**G**   |`true`     |`true`     |`elementIndex == 0 && xLength == yLength` |


And empty spans. `null` spans?

#### Examples
TODO: Add more examples of usage.

```csharp
public static void RepeatEvenIndices(ReadOnlySpan<byte> src, Span<byte> dst)
{
    if (src.Overlaps(dst, out var elementIndex) && elementIndex >= -1)
    {
        throw new ArgumentException();
    }

    for (int i = 0; i < src.Length / 2; i++)
    {
        dst[i + 0] = src[i * 2];
        dst[i + 1] = src[i * 2];
    }
}
```

### Open Questions
Naming is still open. Especially, I am concerned with `Contains` 
being confused with checking if values are contained and not 
whether the slice/span is contained. To be explicit we could 
name these `OverlapsSlice`/`ContainsSlice`.

Could this API be expressed differently, perhaps with an all 
encompassing `Overlap` method returning an enum? 
Performance downsides?

Should the method return `true` or `false` if one or both of the spans are empty?

### Updates
None yet.