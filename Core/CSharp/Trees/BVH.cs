using Core.Geometry;
using Core.Maths.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Core.Trees
{
    public class BVH<TEntry>
    {
        public BVHNode<TEntry>? Root { get; private set; }
        private Func<TEntry, Cuboid3D> _GetBoundingCuboid;
        private Func<TEntry, Vector3D, bool> _IsPointInsideEntry;

        public BVH(List<TEntry>? elements, Func<TEntry, Cuboid3D> getBoundingCuboid, Func<TEntry, Vector3D, bool> isPointInsideEntry)
        {
            _GetBoundingCuboid = getBoundingCuboid;
            _IsPointInsideEntry = isPointInsideEntry;
            if (elements!=null&&elements.Count > 0)
            {
                Root = BuildBVH(elements);
            }
        }

        private BVHNode<TEntry> BuildBVH(List<TEntry> elements)
        {
            if (elements.Count == 1)
            {
                return new BVHNode<TEntry>(_GetBoundingCuboid(elements[0]), null, null, elements);
            }

            Cuboid3D boundingBox = ComputeBoundingBox(elements);
            var splitAxis = GetSplitAxis(boundingBox);
            var sortedElements = elements.OrderBy(e => _GetBoundingCuboid(e).Center.GetAxisValue(splitAxis)).ToList();
            int mid = sortedElements.Count / 2;

            var leftElements = sortedElements.Take(mid).ToList();
            var rightElements = sortedElements.Skip(mid).ToList();

            BVHNode<TEntry> leftChild = BuildBVH(leftElements);
            BVHNode<TEntry> rightChild = BuildBVH(rightElements);

            return new BVHNode<TEntry>(boundingBox, leftChild, rightChild, null);
        }

        private Cuboid3D ComputeBoundingBox(List<TEntry> elements)
        {
            Cuboid3D[] cuboids = elements.Select(e => _GetBoundingCuboid(e)).ToArray();
            Vector3D min = new Vector3D(
                cuboids.Min(e => e.Min.X),
                cuboids.Min(e => e.Min.Y),
                cuboids.Min(e => e.Min.Z));
            Vector3D max = new Vector3D(
                cuboids.Max(e => e.Max.X),
                cuboids.Max(e => e.Max.Y),
                cuboids.Max(e => e.Max.Z));
            return new Cuboid3D(min, max);
        }

        private int GetSplitAxis(Cuboid3D boundingBox)
        {
            var extents = boundingBox.Max - boundingBox.Min;
            if (extents.X > extents.Y && extents.X > extents.Z)
                return 0; // X-axis
            else if (extents.Y > extents.Z)
                return 1; // Y-axis
            else
                return 2; // Z-axis
        }

        public void Insert(TEntry newElement)
        {
            var newElementBoundingBox = _GetBoundingCuboid(newElement);
            if (Root == null) {
                Root = BuildBVH(new List<TEntry> { newElement});
                return;
            }
            Root = InsertIntoNode(Root, newElement, newElementBoundingBox);
        }

        private BVHNode<TEntry> InsertIntoNode(BVHNode<TEntry> node, TEntry newElement, Cuboid3D newElementBoundingBox)
        {
            if (node.Elements != null)
            {
                // Leaf node, add the new element
                node.Elements.Add(newElement);

                // Update the bounding box of the current node
                node.BoundingBox = Cuboid3D.Merge(node.BoundingBox, newElementBoundingBox);

                // Check if the leaf node needs to split (limit of 2 elements for this example)
                if (node.Elements.Count > 2)
                {
                    return SplitNode(node);
                }

                return node;
            }
            else
            {
                // Update the bounding box to include the new element
                node.BoundingBox = Cuboid3D.Merge(node.BoundingBox, newElementBoundingBox);

                // Insert into the appropriate child based on the split axis
                int splitAxis = GetSplitAxis(node.BoundingBox);
                if (_GetBoundingCuboid(newElement).Center.GetAxisValue(splitAxis) <
                    node.BoundingBox.Center.GetAxisValue(splitAxis))
                {
                    node.LeftChild = InsertIntoNode(node.LeftChild, newElement, newElementBoundingBox);
                }
                else
                {
                    node.RightChild = InsertIntoNode(node.RightChild, newElement, newElementBoundingBox);
                }

                return node;
            }
        }

        private BVHNode<TEntry> SplitNode(BVHNode<TEntry> node)
        {
            // Split the elements in the current leaf node
            var elements = node.Elements;
            Cuboid3D boundingBox = ComputeBoundingBox(elements);
            int splitAxis = GetSplitAxis(boundingBox);

            // Sort elements along the chosen split axis
            var sortedElements = elements.OrderBy(e => _GetBoundingCuboid(e).Center.GetAxisValue(splitAxis)).ToList();
            int mid = sortedElements.Count / 2;

            // Split into left and right groups
            var leftElements = sortedElements.Take(mid).ToList();
            var rightElements = sortedElements.Skip(mid).ToList();

            // Create the left and right child nodes
            BVHNode<TEntry> leftChild = BuildBVH(leftElements);
            BVHNode<TEntry> rightChild = BuildBVH(rightElements);

            // Return a new internal node with left and right children
            return new BVHNode<TEntry>(boundingBox, leftChild, rightChild, null);
        }

        public List<TEntry> QueryBVH(Vector3D point)
        {
            if (Root == null) return new List<TEntry>(0);
            return QueryBVHNode(point, Root);
        }

        private List<TEntry> QueryBVHNode(Vector3D point, BVHNode<TEntry> node)
        {
            var result = new List<TEntry>();

            if (node.BoundingBox.Contains(point))
            {
                if (node.Elements != null)
                {
                    foreach (var element in node.Elements)
                    {
                        if (_IsPointInsideEntry(element, point))
                        {
                            result.Add(element);
                        }
                    }
                }
                else
                {
                    if (node.LeftChild != null)
                    {
                        result.AddRange(QueryBVHNode(point, node.LeftChild));
                    }
                    if (node.RightChild != null)
                    {
                        result.AddRange(QueryBVHNode(point, node.RightChild));
                    }
                }
            }

            return result;
        }
    }

    public static class Vector3DExtensions
    {
        public static double GetAxisValue(this Vector3D vector, int axisIndex)
        {
            return axisIndex switch
            {
                0 => vector.X, // X-axis
                1 => vector.Y, // Y-axis
                2 => vector.Z, // Z-axis
                _ => throw new ArgumentOutOfRangeException(nameof(axisIndex), "Axis index must be 0, 1, or 2."),
            };
        }
    }
}
