using Core.Maths;
using Core.Maths.Tensors;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Geometry
{
    public class Plane
    {
        public Vector3D PlanePoint { get; }
        public Vector3D PlaneNormal { get; }
        private double _PlaneNormalDottedWithSelf;
        public Plane(Vector3D planePoint, Vector3D planeNormal)
        {
            PlanePoint = planePoint;
            PlaneNormal = Vector3D.Normalize(planeNormal); // Ensure the normal is normalized
            _PlaneNormalDottedWithSelf = PlaneNormal.Dot(PlaneNormal);
        }
        public Vector3D Get3DPointFromXY(double x, double y, Vector3D uDirection)
        {
            // Step 1: Find a vector 'u' on the plane using the provided uDirection
            Vector3D u = Vector3D.Normalize(uDirection.Subtract(PlaneNormal.Scale(uDirection.Dot(PlaneNormal)))); // Project uDirection onto the plane

            // Step 2: Find a vector 'v' on the plane, perpendicular to 'u'
            Vector3D v = Vector3D.Normalize(PlaneNormal.Cross(u));

            // Step 3: Compute the 3D point corresponding to the (x, y) coordinates
            Vector3D point3D = PlanePoint + (u * x) + (v * y);

            return point3D;
        }
        public Vector3D Get3DPointFromXY(double x, double y)
        {
            // Step 1: Find a vector 'u' on the plane (not parallel to the normal)
            Vector3D u = PlaneNormal.Cross(new Vector3D(1, 0, 0)); // Try using (1,0,0)
            if (u.Magnitude() < 1e-6) // If u is too small (normal was parallel to (1,0,0)), use a different vector
            {
                u = PlaneNormal.Cross(new Vector3D(0, 1, 0)); // Use (0,1,0) if the first attempt failed
            }
            u = Vector3D.Normalize(u);

            // Step 2: Find a vector 'v' on the plane, perpendicular to 'u'
            Vector3D v = Vector3D.Normalize(PlaneNormal.Cross(u));

            // Step 3: Compute the 3D point corresponding to the (x, y) coordinates
            Vector3D point3D = PlanePoint + (u * x) + (v * y);

            return point3D;
        }
        public Vector2D ProjectVectorOntoPlane(Vector3D v) {
            Vector3D projectionOntoN = PlaneNormal.Scale(v.Dot(PlaneNormal) / _PlaneNormalDottedWithSelf);
            Vector3D projected = v.Subtract(projectionOntoN);
            return new Vector2D(projected.X, projected.Y);
        }
    }
}
