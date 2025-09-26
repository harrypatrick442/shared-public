using System;
using System.Linq;
using GlobalConstants;
using Core.Exceptions;
using System.Collections.Generic;
namespace Core
{
    public static class BinarySearchHelper
    {
        public static int? BinarySearchWithOverflowEachEnd<TItem>(IList<TItem> items, 
            Func<TItem, int> compare, out bool wasExactMatch, bool roundUpOnEquals = true, bool exactMatch= false)
        {
            wasExactMatch = false;
            int length = items.Count;
            if (length < 1)
                return null;
            int start = 0, end = length - 1;
            int mid = 0;
            bool greaterThanZero = false;
            while (start <= end)
            {
                mid = (start + end) / 2;
                TItem middleItem = items[mid];
                int compareReturn = compare(middleItem);
                if (compareReturn == 0)
                {
                    wasExactMatch = true;
                    return roundUpOnEquals ? mid + 1 : mid;
                }
                greaterThanZero = compareReturn > 0;
                if (greaterThanZero)
                {
                    start = mid + 1;
                    continue;
                }
                end = mid - 1;
            }
            if (exactMatch) return null;
            return greaterThanZero ? mid + 1 : mid;
        }
    }
}