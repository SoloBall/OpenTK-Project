
using OpenTK.Mathematics;

namespace OpenTK_Project.Logic.Raytracing
{
    public struct ShaderNode
    {
        //public ShaderPrimitive[] Triangles = []; i fucking hate these limitations. Make a new SSBO whenever
        public uint Left; // lie
        public uint Right; // lie
        public uint Parent; // is set automatically or is true
        public uint Self; // is set automatically
        public Vector4 Min, Max;

        // pov: you are tired and create the least future proof code ever, knowing it's gonna bite you in the ass next week
        public ShaderPrimitive triangle1 = new();
        public ShaderPrimitive triangle2 = new();
        public ShaderPrimitive triangle3 = new();
        public ShaderPrimitive triangle4 = new();

        public ShaderNode() { }
    }
}
