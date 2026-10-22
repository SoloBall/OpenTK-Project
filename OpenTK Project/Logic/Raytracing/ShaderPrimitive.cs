using OpenTK.Mathematics;

namespace OpenTK_Project.Logic.Raytracing
{
    public struct ShaderPrimitive
    {
        public Vector4 V0, V1, V2, Normal, Origin;
        public ShaderPrimitive( )
        {
            V0 = new(0, 0, 0, 1.0f);
            V1 = new(0, 0, 0, 1.0f);
            V2 = new(0, 0, 0, 1.0f);
            Normal = new(0, 0, 0, 1.0f);
            Origin = new(0, 0, 0, 1.0f);
        }
    }
}
