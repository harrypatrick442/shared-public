using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Maths.Matrices
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> TakeEveryNth<T>(this IEnumerable<T> source, int nth, int offset=0)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (nth <= 0)
                throw new ArgumentOutOfRangeException(nameof(nth), "n must be greater than 0.");
            int n = offset;
            if (nth == 1) {
                if (offset <= 0) {
                    return source;
                }
                return source.Skip(offset);
            }
            return source.Where((value, index) =>
            {
                if (n >= nth)
                {
                    n = 1;
                    return true;
                }
                n++;
                return false;
            });
        }
    }

}