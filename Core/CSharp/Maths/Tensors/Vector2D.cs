using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Core.Maths.Tensors
{/// <summary>
 /// Represents a 3D vector for spatial calculations.
 /// </summary>
    public class Vector2D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }
        public double Magnitude { get {
                return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
            }
        }
        public static Vector2D Add(Vector2D v1, Vector2D v2)
        {
            return new Vector2D(v1.X + v2.X, v1.Y + v2.Y);
        }
        public Vector2D Add(Vector2D a)
        {
            return Add(this, a);
        }
        public static Vector2D Subtract(Vector2D v1, Vector2D v2)
        {
            return new Vector2D(v1.X - v2.X, v1.Y - v2.Y);
        }
        public Vector2D Subtract(Vector2D a)
        {
            return Subtract(this, a);
        }
        public static Vector2D Divide(Vector2D v1, Vector2D v2)
        {
            return new Vector2D(v1.X / v2.X, v1.Y / v2.Y);
        }
        public Vector2D Divide(Vector2D a)
        {
            return Divide(this, a);
        }
        public static Vector2D Scale(double value, Vector2D direction)
        {
            return new Vector2D(direction.X * value, direction.Y * value);
        }
        public Vector2D Scale(double scalar)
        {
            return Scale(scalar, this);
        }
    }
}