using Core.Maths;
using Core.Maths.Tensors;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Geometry
{
    public class FinitePlane: Plane
    {
        public Vector2D Dimensions { get; }

        public FinitePlane(Vector3D planePoint, Vector3D planeNormal, Vector2D dimensions)
            :base(planePoint, planeNormal)
        {
            Dimensions = dimensions;
        }
    }
}
