using System.Collections;
using System.Collections.Generic;

namespace GoldParserEngine
{
    public class ProductionList : IReadOnlyList<Production>
    {
        private readonly List<Production> _list;

        internal ProductionList() : this(0)
        {
        }

        internal ProductionList(int size)
        {
            _list = new List<Production>();
            for (int n = 0; n < size; n++)
            {
                _list.Add(null);
            }
        }

        /// <summary>
        ///     Returns the production with the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Production this[int index]
        {
            get { return _list[index]; }

            internal set { _list[index] = value; }
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<Production> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _list).GetEnumerator();
        }

        /// <summary>
        ///     Gets the number of elements in the collection.
        /// </summary>
        /// <returns>
        ///     The number of elements in the collection.
        /// </returns>
        public int Count
        {
            get { return _list.Count; }
        }

        internal void Clear()
        {
            _list.Clear();
        }
    }
}