using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
namespace Core.Sorting
{
    public class SortWithUnknownsCircularException<TPayload>:Exception
    {
        private SortWithUnknownsNode<TPayload>[] _Circle;
        public SortWithUnknownsNode<TPayload>[] Circle { get { return _Circle; } }
        public SortWithUnknownsCircularException(SortWithUnknownsNode<TPayload>[] circle):base(GetMessage(circle))
        {
            _Circle = circle;
        }
        private static string GetMessage(SortWithUnknownsNode<TPayload>[] circle)
        {
            return $"Was unable to resolve a circular loop of entries {string.Join(" -> ", circle.Select(sortWithUnknownsNode=>(sortWithUnknownsNode.GetName())))}-> {(circle[0].GetName())}";
        }
    }
}