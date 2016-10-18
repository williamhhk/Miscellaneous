namespace Functional.Monads
{
    using System.Collections.Generic;
    public class Maybe<T> : IEnumerable<T>
    {
        private readonly T[] data;

        private Maybe(T[] data)
        {
            this.data = data;
        }

        public static Maybe<T> Create(T element)
        {
            return new Maybe<T>(new T[] { element });
        }

        public static Maybe<T> CreateEmpty()
        {
            return new Maybe<T>(new T[0]);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)this.data).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
