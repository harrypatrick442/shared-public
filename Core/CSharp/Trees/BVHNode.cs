using Core.Geometry;
using System.Collections.Generic;

namespace Core.Trees
{
    public class BVHNode<TEntry>
    {
        public Cuboid3D BoundingBox { get; set; }
        public BVHNode<TEntry> LeftChild { get; set; }
        public BVHNode<TEntry> RightChild { get; set; }
        public List<TEntry> Elements { get; private set; }

        public BVHNode(Cuboid3D boundingBox, BVHNode<TEntry> leftChild, BVHNode<TEntry> rightChild,
            List<TEntry> elements)
        {
            BoundingBox = boundingBox;
            LeftChild = leftChild;
            RightChild = rightChild;
            Elements = elements;
        }
    }

}