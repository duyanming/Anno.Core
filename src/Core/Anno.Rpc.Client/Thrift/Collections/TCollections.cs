using System;
using System.Collections;

namespace Thrift.Collections
{
    public class TCollections
    {
        /// <summary>
        /// This will return true if the two collections are value-wise the same.
        /// If the collection contains a collection, the collections will be compared using this method.
        /// </summary>
        public static Boolean Equals(IEnumerable first, IEnumerable second)
        {
            if (first == null && second == null)
            {
                return true;
            }
            if (first == null || second == null)
            {
                return false;
            }
            var fiter = first.GetEnumerator();
            var siter = second.GetEnumerator();

            var fnext = fiter.MoveNext();
            var snext = siter.MoveNext();
            while (fnext && snext)
            {
                var fenum = fiter.Current as IEnumerable;
                var senum = siter.Current as IEnumerable;
                if (fenum != null && senum != null)
                {
                    if (!Equals(fenum, senum))
                    {
                        return false;
                    }
                }
                else if (fenum == null ^ senum == null)
                {
                    return false;
                }
                else if (!Equals(fiter.Current, siter.Current))
                {
                    return false;
                }
                fnext = fiter.MoveNext();
                snext = siter.MoveNext();
            }

            return fnext == snext;
        }

        /// <summary>
        /// This returns a hashcode based on the value of the enumerable.
        /// </summary>
        public static Int32 GetHashCode(IEnumerable enumerable)
        {
            if (enumerable == null)
            {
                return 0;
            }

            var hashcode = 0;
            foreach (var obj in enumerable)
            {
                var enum2 = obj as IEnumerable;
                var objHash = enum2 == null ? obj.GetHashCode() : GetHashCode(enum2);
                unchecked
                {
                    hashcode = (hashcode * 397) ^ (objHash);
                }
            }
            return hashcode;
        }
    }
}