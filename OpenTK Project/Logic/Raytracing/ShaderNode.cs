
using OpenTK.Mathematics;

namespace OpenTK_Project.Logic.Raytracing
{
    public struct ShaderNode
    {
        public TrianglePrimitive[] Triangles = [];
        public uint Left = 0; // lie
        public uint Right = 0; // lie
        public uint Parent = 0; // is set automatically or is true
        public uint Self = 0; // is set automatically
        public Vector4 Min, Max;

        public ShaderNode() { }
    }
}
