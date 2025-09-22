using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Core.Maths.Tensors
{/// <summary>
 /// Represents a 3D vector for spatial calculations.
 /// </summary>
    public class Vector3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Calculates the magnitude of the vector.
        /// </summary>
        public double Magnitude()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }
        // Dot product of two vectors
        public double Dot(Vector3D other)
        {
            return this.X * other.X + this.Y * other.Y + this.Z * other.Z;
        }

        /// <summary>
        /// Adds two vectors and returns the result.
        /// </summary>
        public static Vector3D Add(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }
        /// <summary>
        /// Subtracts the second vector from the first vector and returns the result.
        /// </summary>
        public static Vector3D Subtract(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }
        public Vector3D Subtract(Vector3D a)
        {
            return Subtract(this, a);
        }

        /// <summary>
        /// Scales the vector by a scalar factor.
        /// </summary>
        /// <param name="scalar">The factor by which to scale each component of the vector.</param>
        /// <returns>A new Vector3D that is the result of scaling the original vector.</returns>
        public Vector3D Scale(double scalar)
        {
            return Scale(scalar, this);
        }
        public static Vector3D Scale(double value, Vector3D direction)
        {
            return new Vector3D(direction.X * value, direction.Y * value, direction.Z * value);
        }
        public static Vector3D MultiplyComponentWise(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }
        public static Vector3D Zeros()
        {
            return new Vector3D(0, 0, 0);
        }
        public static Vector3D operator +(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        public static Vector3D operator /(Vector3D a, double scalar)
        {
            return new Vector3D(a.X / scalar, a.Y / scalar, a.Z / scalar);
        }
        // Scalar multiplication
        public static Vector3D operator *(Vector3D a, double scalar)
        {
            return new Vector3D(a.X * scalar, a.Y * scalar, a.Z * scalar);
        }
        public static Vector3D operator -(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        // Cross product
        public Vector3D Cross(Vector3D other)
        {
            return new Vector3D(
                Y * other.Z - Z * other.Y,
                Z * other.X - X * other.Z,
                X * other.Y - Y * other.X
            );
        }
        public Vector3D Normalize() {
            return Normalize(this);
        }
        public static Vector3D Normalize(Vector3D a)
        {
            double mag = a.Magnitude();
            return new Vector3D(a.X / mag, a.Y / mag, a.Z / mag);
        }
        public double[] ToArray() { 
            return new double[] { X, Y, Z };
        }
        public double DistanceTo(Vector3D other)
        {
            double dx = this.X - other.X;
            double dy = this.Y - other.Y;
            double dz = this.Z - other.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        public static Vector3D FromArray(double[] values) {
            if (values.Length != 3) {
                throw new ArgumentException($"{nameof(values)}");
            }
            return new Vector3D(values[0], values[1], values[2]);
        }
        public override bool Equals(object? obj)
        {
            // Check if obj is a Vector3D and cast it
            if (obj is Vector3D other)
            {
                // Return true if all components match
                return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
            }
            // Return false if obj is not a Vector3D
            return false;
        }
        public override string ToString()
        {
            return $"[{X}, {Y}, {Z}]";
        }
    }
}