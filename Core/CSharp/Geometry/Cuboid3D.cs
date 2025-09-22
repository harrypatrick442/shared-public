using System;
using Core.Maths.Tensors;

namespace Core.Geometry
{
    public class Cuboid3D
    {
        public Vector3D Min { get; private set; } // Minimum corner of the cuboid (xMin, yMin, zMin)
        public Vector3D Max { get; private set; } // Maximum corner of the cuboid (xMax, yMax, zMax)

        public double XMin { get { return Min.X; } }
        public double XMax { get { return Max.X; } }
        public double YMin { get { return Min.Y; } }
        public double YMax { get { return Max.Y; } }
        public double ZMin { get { return Min.Z; } }
        public double ZMax { get { return Max.Z; } }

        public Cuboid3D(Vector3D min, Vector3D max)
        {
            Min = min;
            Max = max;
            if (min.X >= max.X || min.Y >= max.Y || min.Z >= max.Z)
                throw new Exception("Invalid volume");
        }

        public Cuboid3D(Vector3D center, double width, double height, double depth)
        {
            double halfWidth = width / 2.0;
            double halfHeight = height / 2.0;
            double halfDepth = depth / 2.0;

            Min = new Vector3D(center.X - halfWidth, center.Y - halfHeight, center.Z - halfDepth);
            Max = new Vector3D(center.X + halfWidth, center.Y + halfHeight, center.Z + halfDepth);
        }

        public Vector3D Center => new Vector3D((Min.X + Max.X) / 2.0, (Min.Y + Max.Y) / 2.0, (Min.Z + Max.Z) / 2.0);

        public double Width => Max.X - Min.X;
        public double Height => Max.Y - Min.Y;
        public double Depth => Max.Z - Min.Z;

        public bool Contains(Vector3D point)
        {
            return (point.X >= Min.X && point.X <= Max.X) &&
                   (point.Y >= Min.Y && point.Y <= Max.Y) &&
                   (point.Z >= Min.Z && point.Z <= Max.Z);
        }

        public bool Intersects(Cuboid3D other, double epsilon = 1e-10)
        {
            // X-axis overlap check
            double xMinDifference = other.Max.X - Min.X;
            double xMaxDifference = Max.X - other.Min.X;
            bool xIntersected = xMinDifference >= -epsilon && xMaxDifference >= -epsilon;

            // Y-axis overlap check
            double yMinDifference = other.Max.Y - Min.Y;
            double yMaxDifference = Max.Y - other.Min.Y;
            bool yIntersected = yMinDifference >= -epsilon && yMaxDifference >= -epsilon;

            // Z-axis overlap check
            double zMinDifference = other.Max.Z - Min.Z;
            double zMaxDifference = Max.Z - other.Min.Z;
            bool zIntersected = zMinDifference >= -epsilon && zMaxDifference >= -epsilon;

            return xIntersected && yIntersected && zIntersected;
        }

        // Static method to construct a Cuboid3D using center and halfSize
        public static Cuboid3D ConstructFromCenterAndHalfSize(Vector3D center, double halfSize)
        {
            Vector3D min = new Vector3D(
                center.X - halfSize,
                center.Y - halfSize,
                center.Z - halfSize
            );

            Vector3D max = new Vector3D(
                center.X + halfSize,
                center.Y + halfSize,
                center.Z + halfSize
            );

            return new Cuboid3D(min, max);
        }

        // Merge method to combine two cuboids into one that encompasses both
        public static Cuboid3D Merge(Cuboid3D cuboid1, Cuboid3D cuboid2)
        {
            Vector3D newMin = new Vector3D(
                Math.Min(cuboid1.Min.X, cuboid2.Min.X),
                Math.Min(cuboid1.Min.Y, cuboid2.Min.Y),
                Math.Min(cuboid1.Min.Z, cuboid2.Min.Z)
            );

            Vector3D newMax = new Vector3D(
                Math.Max(cuboid1.Max.X, cuboid2.Max.X),
                Math.Max(cuboid1.Max.Y, cuboid2.Max.Y),
                Math.Max(cuboid1.Max.Z, cuboid2.Max.Z)
            );

            return new Cuboid3D(newMin, newMax);
        }

        public override string ToString()
        {
            return $"Cuboid3D(Min: {Min}, Max: {Max})";
        }
    }
}
