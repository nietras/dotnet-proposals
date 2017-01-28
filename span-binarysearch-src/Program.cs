using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System
{
    public struct Span<T> 
    { 
        public static implicit operator ReadOnlySpan<T>(Span<T> span) => throw new NotImplementedException();
    }

    public struct ReadOnlySpan<T> { }

    public static class ReadOnlySpanExtensions
    {
        //public static int BinarySearch<T>(this ReadOnlySpan<T> span, T value) 
        //{ throw null; }

        public static int BinarySearch<T, TComparable>(this ReadOnlySpan<T> span, TComparable comparable) 
            where TComparable : IComparable<T> 
        { throw null; }

        public static int BinarySearch<T, TComparer>(this ReadOnlySpan<T> span, T value, TComparer comparer) 
            where TComparer : IComparer<T>
        { throw null; }
    }

    public static class Usage
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

        public static void BinarySearch()
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
    }

    public static class Program
    {
        public static void Main()
        {
            Usage.BinarySearch();
        }
    }
}