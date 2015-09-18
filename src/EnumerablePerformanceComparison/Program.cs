using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.LinqTest.New;

namespace EnumerablePerformance.Comparison
{
    class Program
    {
        private const int Iterations = 10000;
        private static IEnumerable<int> Range(int count)
        {
            for (int i = 0; i != count; ++i) yield return i;
        }
        public static void Main(string[] args)
        {
            foreach (int size in new[] { 10, 16, 100, 128, 1000, 1024 })
            {
                MeasurePremutations(Range(size));
                Console.Error.WriteLine("int");
                MeasurePremutations(Range(size).Select(i => (long)i));
                Console.Error.WriteLine("long");
                MeasurePremutations(Range(size).Select(i => i.ToString()));
                Console.Error.WriteLine("string");
                MeasurePremutations(Range(size).Select(i => new DateTime((long)i)));
                Console.Error.WriteLine("DateTime");
                MeasurePremutations(Range(size).Select(i => (object)i));
                Console.Error.WriteLine("boxed int");
                Console.WriteLine();
                Console.Error.WriteLine(size);
            }
        }
        private static void MeasurePremutations<T>(IEnumerable<T> basis)
        {
            foreach (var seq in Permutations(basis))
            {
                bool flip = false;
                Console.Write(TimeToArray(seq, i => i));
                Console.Write(',');
                Console.Write(TimeToArray(seq, i => true));
                Console.Write(',');
                Console.Write(TimeToArray(seq, i => true, i => i));
                Console.Write(',');
                Console.Write(TimeToArray(seq, i => false));
                Console.Write(',');
                Console.Write(TimeToArray(seq, i => false, i => i));
                Console.Write(',');
                Console.Write(TimeToArray(seq, i => flip = !flip));
                Console.Write(',');
                Console.Write(TimeToArray(seq, i => flip = !flip, i => i));
                Console.Write(',');
                Console.Write(TimeToConsume(seq, i => i));
                Console.Write(',');
                Console.Write(TimeToConsume(seq, i => true));
                Console.Write(',');
                Console.Write(TimeToConsume(seq, i => true, i => i));
                Console.Write(',');
                Console.Write(TimeToConsume(seq, i => false));
                Console.Write(',');
                Console.Write(TimeToConsume(seq, i => false, i => i));
                Console.Write(',');
                Console.Write(TimeToConsume(seq, i => flip = !flip));
                Console.Write(',');
                Console.Write(TimeToConsume(seq, i => flip = !flip, i => i));
                Console.Write(',');
            }
        }
        private static void PreemptGC()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        private static IEnumerable<IEnumerable<T>> Permutations<T>(IEnumerable<T> basis)
        {
            yield return basis;
            yield return basis.ToArray();
            yield return basis.ToList();
            yield return basis.ToList().AsReadOnly();
            yield return new ListCopy<T>(basis);
        }
        private static long TimeToArray<TSource>(IEnumerable<TSource> source, Func<TSource, TSource> selector)
        {
            source.Select(selector).ToArray();
            PreemptGC();
            var sw = Stopwatch.StartNew();
            for (int i = 0; i != Iterations; ++i)
                source.Select(selector).ToArray();
            sw.Stop();
            return sw.ElapsedTicks;
        }
        private static long TimeToArray<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            source.Where(predicate).ToArray();
            PreemptGC();
            var sw = Stopwatch.StartNew();
            for (int i = 0; i != Iterations; ++i)
                source.Where(predicate).ToArray();
            sw.Stop();
            return sw.ElapsedTicks;
        }
        private static long TimeToArray<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TSource, TSource> selector)
        {
            source.Where(predicate).Select(selector).ToArray();
            PreemptGC();
            var sw = Stopwatch.StartNew();
            for (int i = 0; i != Iterations; ++i)
                source.Where(predicate).Select(selector).ToArray();
            sw.Stop();
            return sw.ElapsedTicks;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Consume<T>(IEnumerable<T> sequence)
        {
            using (var en = sequence.GetEnumerator())
                while (en.MoveNext())
                {
                }
        }
        private static long TimeToConsume<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            Consume(source.Select(selector));
            PreemptGC();
            var sw = Stopwatch.StartNew();
            for (int i = 0; i != Iterations; ++i)
                Consume(source.Select(selector));
            sw.Stop();
            return sw.ElapsedTicks;
        }
        private static long TimeToConsume<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Consume(source.Where(predicate));
            PreemptGC();
            var sw = Stopwatch.StartNew();
            for (int i = 0; i != Iterations; ++i)
                Consume(source.Where(predicate));
            sw.Stop();
            return sw.ElapsedTicks;
        }
        private static long TimeToConsume<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
        {
            Consume(source.Where(predicate).Select(selector));
            PreemptGC();
            var sw = Stopwatch.StartNew();
            for (int i = 0; i != Iterations; ++i)
                Consume(source.Where(predicate).Select(selector));
            sw.Stop();
            return sw.ElapsedTicks;
        }
    }
}