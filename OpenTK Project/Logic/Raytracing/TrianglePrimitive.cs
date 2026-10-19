using OpenTK.Mathematics;

namespace OpenTK_Project.Logic.Raytracing
{
    public struct TrianglePrimitive
    {
        public Vector4 V0, V1, V2, Normal, Origin, Min, Max, Centroid;
        public Color4 Color; // 16 bytes, matches vec4 in std430
        public Quaternion Orientation;
    }
}
