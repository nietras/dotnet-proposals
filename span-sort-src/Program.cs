using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System
{
    public struct Span<T> { }

    public static class SpanExtensions
    {
        public static void Sort<T>(this Span<T> span)
        { }

        public static void Sort<T, TComparer>(this Span<T> span, TComparer comparer)
           where TComparer : IComparer<T>
        { }

        public static void Sort<T>(this Span<T> span, System.Comparison<T> comparison)
        { }

        public static void Sort<TKey, TValue>(this Span<TKey> keys, Span<TValue> items)
        { }

        public static void Sort<TKey, TValue, TComparer>(this Span<TKey> keys,
           Span<TValue> items, TComparer comparer)
           where TComparer : IComparer<TKey>
        { }

        public static void Sort<TKey, TValue>(this Span<TKey> keys,
           Span<TValue> items, System.Comparison<TKey> comparison)
        { }
    }

    public static class Usage
    {
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

        public static void SingleSpan()
        {
            var span = new Span<int>();

            span.Sort();

            // Inlineable struct comparer
            var reverseComparer = new ReverseComparer();
            span.Sort(reverseComparer);

            // Lambda comparer
            span.Sort((a, b) => a == b ? 0 : (a > b ? -1 : 1));
        }

        public static void TwoSpans()
        {
            var keys = new Span<int>();
            var items = new Span<double>();
            keys.Sort(items);

            // Inlineable struct comparer
            var reverseComparer = new ReverseComparer();
            keys.Sort(items, reverseComparer);

            keys.Sort(items, (a, b) => a == b ? 0 : (a > b ? -1 : 1));
        }
    }

    public static class Program
    {
        public static void Main()
        {

        }
    }
}