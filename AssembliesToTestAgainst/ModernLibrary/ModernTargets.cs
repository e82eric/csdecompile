using System;
using System.Collections.Generic;

namespace ModernLibrary;

public record ModernPerson(string Name, int Age);

public class ModernOuter
{
    public class Inner
    {
        public void InnerMethod() { }
        public event EventHandler? InnerEvent;
        public int InnerField;
        public int InnerProp { get; set; }
        public void Raise() => InnerEvent?.Invoke(this, EventArgs.Empty);

        public class Deepest
        {
            public void DeepestMethod() { }
        }
    }

    public event EventHandler? OuterEvent;
    public int this[int i] => i;
    public int this[string s] => 0;
    public required string Req { get; init; }
    public void Raise() => OuterEvent?.Invoke(this, EventArgs.Empty);
}

public class ModernGeneric<T>
{
    public event EventHandler? GenEvent;
    public void Take(T item) { }
    public T? GenProp { get; set; }
    public void Raise() => GenEvent?.Invoke(this, EventArgs.Empty);
}

public class ModernPrimary(int seed)
{
    public int Next() => seed + 1;
}

public static class ModernApi
{
    public static void ArrInt(int[] values) { }
    public static void Arr(int[] values) { }
    public static void Arr(string[] values) { }
    public static void Jagged(int[][] values) { }
    public static void Jagged(int[,] values) { }
    public static void MaybeInt(int? value) { }
    public static void Seq(IEnumerable<string> values) { }
    public static void Seq(IEnumerable<int> values) { }
    public static void Nested(Dictionary<string, List<int>> values) { }
    public static void Nested(Dictionary<string, List<string>> values) { }
    public static void Arity(int x) { }
    public static void Arity<T>(int x) { }
    public static void Arity<T1, T2>(int x) { }
    public static void ByRef(int x) { }
    public static void ByRef(ref int x) { }
    public static void GenericParam<T>(T item) { }
    public static void GenericParam<T>(List<T> items) { }
    public static void Spans(ReadOnlySpan<char> s) { }
    public static void ParamsSpan(params ReadOnlySpan<int> xs) { }
    public static void Tuples((int a, string b) pair) { }
    public static void Named(int first, int second) { }
    public static int Classic(this string s) => s.Length;
}

public static class ModernExtensions
{
    extension(string s)
    {
        public int Twice => s.Length * 2;
        public int Thrice() => s.Length * 3;
    }

    public static int Old(this string s) => 1;
}
