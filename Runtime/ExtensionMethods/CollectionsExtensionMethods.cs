using System.Collections.Generic;
using System.Linq;

namespace SensenToolkit
{
    public static class CollectionsExtensionMethods
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => UnityEngine.Random.value);
        }

        public static IEnumerable<T> GetRandomElements<T>(this IEnumerable<T> source, int amount)
        {
            return source.Shuffle().Take(amount);
        }

        public static T GetRandomElement<T>(this IEnumerable<T> source)
        {
            int count = source.Count();
            if (count == 0) return default;

            int index = UnityEngine.Random.Range(0, count);
            return source.ElementAt(index);
        }

        public static Stack<T> ToStack<T>(this IEnumerable<T> source)
        {
            return new(source);
        }

        public static Queue<T> ToQueue<T>(this IEnumerable<T> source)
        {
            return new(source);
        }

        public static IEnumerable<IEnumerable<T>> SlicesOf<T>(this IEnumerable<T> source, int size)
        {
            List<T> list = new(size);
            foreach (T item in source)
            {
                list.Add(item);
                if (list.Count == size)
                {
                    yield return list;
                    list = new(size);
                }
            }
            if (list.Count > 0)
            {
                yield return list;
            }
        }

        public static IEnumerable<IEnumerable<T>> SliceByAmount<T>(this IEnumerable<T> source, int amount)
        {
            int size = source.Count() / amount;
            return source.SlicesOf(size);
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
        {
            foreach (IEnumerable<T> subset in source)
            {
                foreach (T item in subset)
                {
                    yield return item;
                }
            }
        }
    }
}
