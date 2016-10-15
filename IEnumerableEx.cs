using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Methods
{
    public static class EnumerableEx
    {
        public static IEnumerable<T> AsEnumerable<T>(this IEnumerator source)
        {
            IEnumerable<T> collections = Enumerable.Empty<T>();
            while (source.MoveNext())
            {
                var t = (T)source.Current;
                collections = collections.Concat(new[] { t });
            }
            return collections;
        }

        public static void Each<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }

        public static IEnumerable<Tuple<string, string>> OrderByGroup(this IEnumerable<Tuple<string, string>> source)
        {
            return source.OrderBy(item => item.Item1);
        }

        public static IEnumerable<T> LazyDefaultIfEmpty<T>(this IEnumerable<T> source,
                                                           Func<T> defaultFactory)
        {
            bool isEmpty = true;
            foreach (T value in source)
            {
                yield return value;
                isEmpty = false;
            }

            if (isEmpty)
                yield return defaultFactory();
        }

        public static IEnumerable<TIn> Catch<TIn>(this IEnumerable<TIn> source)
        {
            using (var e = source.GetEnumerator())
                while (true)
                {
                    var ok = false;
                    try
                    {
                        ok = e.MoveNext();
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (!ok)
                        yield break;

                    yield return e.Current;
                }
        }

        public static IEnumerable<TIn> Catch<TIn>(this IEnumerable<TIn> source, Type exceptionType)
        {
            using (var e = source.GetEnumerator())
                while (true)
                {
                    var ok = false;
                    try
                    {
                        ok = e.MoveNext();
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType() != exceptionType)
                            throw;
                        continue;
                    }
                    if (!ok)
                        yield break;
                    yield return e.Current;
                }
        }
    }
}
