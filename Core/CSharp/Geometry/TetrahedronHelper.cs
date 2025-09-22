using Core.Maths.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Geometry
{
    public static class TetrahedronHelper
    {
        public static double SignedVolume(Vector3D a, Vector3D b, Vector3D c, Vector3D d)
        {
            return (1.0 / 6.0) * (
                (a.X - d.X) * ((b.Y - d.Y) * (c.Z - d.Z) - (b.Z - d.Z) * (c.Y - d.Y)) -
                (a.Y - d.Y) * ((b.X - d.X) * (c.Z - d.Z) - (b.Z - d.Z) * (c.X - d.X)) +
                (a.Z - d.Z) * ((b.X - d.X) * (c.Y - d.Y) - (b.Y - d.Y) * (c.X - d.X))
            );
        }
        public static double AbsoluteVolume(Vector3D v1, Vector3D v2, Vector3D v3, Vector3D v4)
        {
            // Define vectors a, b, c
            Vector3D a = v2 - v1;
            Vector3D b = v3 - v1;
            Vector3D c = v4 - v1;

            // Calculate the cross product of b and c
            Vector3D crossProduct = new Vector3D(
                b.Y * c.Z - b.Z * c.Y,
                b.Z * c.X - b.X * c.Z,
                b.X * c.Y - b.Y * c.X
            );

            // Calculate the dot product of a and (b x c)
            double dotProduct = a.X * crossProduct.X + a.Y * crossProduct.Y + a.Z * crossProduct.Z;

            // Volume is 1/6th of the absolute value of the scalar triple product
            double volume = Math.Abs(dotProduct) / 6.0;

            return volume;
        }
    }
}
