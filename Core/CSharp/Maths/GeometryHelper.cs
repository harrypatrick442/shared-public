using Core.Maths.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Maths
{
    public class GeometryHelper
    {
        public static double TriangleArea(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            return (x1 * y2 + x2 * y3 + x3 * y1 - x1 * y3 - x2 * y1 - x3 * y2) / 2d;
        }
        public static double TriangleArea(double x1, double y1, double z1,
                                               double x2, double y2, double z2,
                                               double x3, double y3, double z3)
        {
            // Calculate the vectors AB and AC
            double ABx = x2 - x1;
            double ABy = y2 - y1;
            double ABz = z2 - z1;

            double ACx = x3 - x1;
            double ACy = y3 - y1;
            double ACz = z3 - z1;

            // Compute the cross product AB x AC
            double crossProductX = ABy * ACz - ABz * ACy;
            double crossProductY = ABz * ACx - ABx * ACz;
            double crossProductZ = ABx * ACy - ABy * ACx;

            // Calculate the magnitude of the cross product (which is 2 times the area of the triangle)
            double crossProductMagnitude = Math.Sqrt(crossProductX * crossProductX +
                                                     crossProductY * crossProductY +
                                                     crossProductZ * crossProductZ);

            // The area of the triangle is half the magnitude of the cross product
            double area = 0.5 * crossProductMagnitude;

            return area;
        }
        public static double VolumeTetrahedron(Vector3D a, Vector3D b, Vector3D c, Vector3D d)
        {
            return VolumeTetrahedron(a.X, a.Y, a.Z, b.X, b.Y, b.Z, c.X, c.Y, c.Z, d.X, d.Y, d.Z);
        }

        public static double VolumeTetrahedron(
            double x1, double y1, double z1,
            double x2, double y2, double z2,
            double x3, double y3, double z3,
            double x4, double y4, double z4)
        {
            return Math.Abs(MatrixHelper.Determinant(new double[][]{
                new double[]{x1, y1, z1, 1},
                new double[]{x2, y2, z2, 1},
                new double[]{x3, y3, z3, 1},
                new double[]{x4, y4, z4, 1}
            }) / 6d);
        }
    }
}