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

    public static class SpanExtensions
    {
        // Convenience overload
        public static int BinarySearch<T>(
            this ReadOnlySpan<T> span, IComparable<T> comparable) 
        { return BinarySearch<T, IComparable<T>>(span, comparable); }

        public static int BinarySearch<T, TComparable>(
            this ReadOnlySpan<T> span, TComparable comparable) 
            where TComparable : IComparable<T> 
        { return -1; }

        public static int BinarySearch<T, TComparer>(
            this ReadOnlySpan<T> span, T value, TComparer comparer) 
            where TComparer : IComparer<T>
        { return -1; }

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

    public static class UsageForInt
    {
        struct InlineableIntComparer : IComparer<int>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(int a, int b)
            {
                if (a == b) { return 0; }
                if (a < b) { return -1; }
                return 1;
            }
        }

        struct InlineableIntComparable : IComparable<int>
        {
            int m_value;
            
            public InlineableIntComparable(int value)
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

        class IntComparable : IComparable<int>
        {
            int m_value;
            
            public IntComparable(int value)
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

            // Comparable
            var comparable = new IntComparable(valueToFind);
            index = span.BinarySearch(comparable);

            // IComparable
            IComparable<int> icomparable = new IntComparable(valueToFind);
            index = span.BinarySearch(icomparable);

            // Inlineable struct comparable
            var inlineableComparable = new InlineableIntComparable(valueToFind);
            index = span.BinarySearch(inlineableComparable);

            // Inlineable struct comparer
            var inlineableComparer = new InlineableIntComparer();
            index = span.BinarySearch(valueToFind, inlineableComparer);

            // Default comparer
            index = span.BinarySearch(valueToFind, Comparer<int>.Default);
        }

        public static void ReadOnlySpanBinarySearch()
        {
            ReadOnlySpan<int> span = new Span<int>();

            //span.Sort();

            int valueToFind = 42;

            // Direct value
            var index = span.BinarySearch(valueToFind);
            
            // Comparable
            var comparable = new IntComparable(valueToFind);
            index = span.BinarySearch(comparable);

            // IComparable
            IComparable<int> icomparable = new IntComparable(valueToFind);
            index = span.BinarySearch(icomparable);

            // Inlineable struct comparable
            var inlineableComparable = new InlineableIntComparable(valueToFind);
            index = span.BinarySearch(inlineableComparable);

            // Inlineable struct comparer
            var inlineableComparer = new InlineableIntComparer();
            index = span.BinarySearch(valueToFind, inlineableComparer);

            // Default comparer
            index = span.BinarySearch(valueToFind, Comparer<int>.Default);
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