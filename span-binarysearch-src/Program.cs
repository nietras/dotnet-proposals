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
        // Convenience overload
        public static int BinarySearch<T>(this ReadOnlySpan<T> span, IComparable<T> comparable) 
        { return BinarySearch<T, IComparable<T>>(span, comparable); }

        public static int BinarySearch<T, TComparable>(this ReadOnlySpan<T> span, TComparable comparable) 
            where TComparable : IComparable<T> 
        { throw null; }

        public static int BinarySearch<T, TComparer>(this ReadOnlySpan<T> span, T value, TComparer comparer) 
            where TComparer : IComparer<T>
        { throw null; }
    }

    public static class SpanExtensions
    {
        // Convenience overload
        public static int BinarySearch<T>(this Span<T> span, IComparable<T> comparable) 
        { return BinarySearch<T, IComparable<T>>(span, comparable); }

        // NOTE: Due to the less-than-ideal generic type inference in the face of implicit conversions,
        //       we need the overloads taking Span<T>. These simply forward to ReadOnlySpanExtensions.
        public static int BinarySearch<T, TComparable>(this Span<T> span, TComparable comparable) 
            where TComparable : IComparable<T> 
        { return ReadOnlySpanExtensions.BinarySearch<T, TComparable>(span, comparable); }

        public static int BinarySearch<T, TComparer>(this Span<T> span, T value, TComparer comparer) 
            where TComparer : IComparer<T>
        { return ReadOnlySpanExtensions.BinarySearch(span, value, comparer); }
    }

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
#pragma warning disable 0649 // Unused/unassigned warning, not important here
        struct Compound
        {
            public float FeatureValue;
            public int FeatureIndex;
            public object Payload;
        }
#pragma warning restore 0649

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

    public static class Program
    {
        public static void Main()
        {
            UsageForInt.SpanBinarySearch();
            UsageForInt.ReadOnlySpanBinarySearch();

            UsageForCompound.SpanBinarySearch();
            UsageForCompound.ReadOnlySpanBinarySearch();
        }
    }
}